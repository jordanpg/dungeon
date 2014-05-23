//thanks to plornt for reference in a good bit of this (duplorcator)
$DungeonCellTypes = "sdown sup floor wall corner door decor";

//Credit to Space Guy for this
function getClosestPaintColor(%rgba)
{
	%prevdist = 100000;
	%colorMatch = 0;
	for(%i = 0;%i < 64;%i++)
	{
		%color = getColorIDTable(%i);
		if(vectorDist(%rgba,getWords(%color,0,2)) < %prevdist && getWord(%rgba,3) - getWord(%color,3) < 0.3 && getWord(%rgba,3) - getWord(%color,3) > -0.3)
		{
			%prevdist = vectorDist(%rgba,%color);
			%colormatch = %i;
		}
	}
	return %colormatch;
}

function angleToRot(%id)
{
	switch(%id)
	{
		case 0: %rotation = "1 0 0 0";
		case 1: %rotation = "0 0 1 90";
		case 2: %rotation = "0 0 1 180";
		case 3: %rotation = "0 0 -1 90";
		default: %rotation = "1 0 0 0";
	}
	return %rotation;
}

function rotateVector(%vector, %axis, %val)
{
	switch(%val)
	{
		case 1:
			%nX = getWord(%axis, 0) + (getWord(%vector, 1) - getWord(%axis, 1));
			%nY = getWord(%axis, 1) - (getWord(%vector, 0) - getWord(%axis, 0));
			%new = %nX SPC %nY SPC getWord(%vector, 2);
		case 2:
			%nX = getWord(%axis, 0) - (getWord(%vector, 0) - getWord(%axis, 0));
			%nY = getWord(%axis, 1) - (getWord(%vector, 1) - getWord(%axis, 1));
			%new = %nX SPC %nY SPC getWord(%vector, 2);
		case 3:
			%nX = getWord(%axis, 0) - (getWord(%vector, 1) - getWord(%axis, 1));
			%nY = getWord(%axis, 1) + (getWord(%vector, 0) - getWord(%axis, 0));
			%new = %nx SPC %nY SPC getWord(%vector, 2);
		default: %new = vectorAdd(%vector, %axis);
	}
	return %new;
}

function findWord(%string, %searchWord)
{
	%searchWord = firstWord(%searchWord);
	%wordCt = getWordCount(%string);
	for(%i = 0; %i < %wordCt; %i++)
	{
		%word = getWord(%string, %i);
		if(%word $= %searchWord)
			return %i;
	}
	return -1;
}

function newBLSObject(%file)
{
	if(!isFile(%file) || fileExt(%file) !$= ".bls")
		%file = "";

	%this = new ScriptObject(BLSObject)
	{
		saveFile = %file;
		colors = 0;

		loadOffset = "0 0 0";
		loadRotation = 0;
		loadSpeed = 150;
		brickGroup = BrickGroup_888888;
		axis = "0 0 0";
		reset = true;
		ignoreErrors = false;
	};
	return %this;
}

function BLSObject::fetchLoadInformation(%self)
{
	%file = new FileObject();
	%file.openForRead(%self.saveFile);
	for(%i = 0; !%file.isEOF(); %i++)
	{
		%line = %file.readLine();

		if(firstWord(%line) $= "Linecount")
		{

			break; //we don't want anything past linecount for this
		}
		else if(%i >= 3)
		{
			%self.color[%self.colors] = findClosestColor(%line);
			%self.colors++;
		}
	}
	%file.close();
	%file.delete();
}

function BLSObject::setLoadParameters(%self, %offset, %rot, %speed, %file, %bg, %centre, %reset, %err)
{
	%params = 0;
	if(%offset !$= "")
	{
		%self.loadOffset = %offset;
		%params++;
	}
	if(%rot !$= "")
	{
		if(%rot < 0)
			%self.loadRotation = %rot + 4;
		else if(%rot > 3)
			%self.loadRotation = %rot - 4;
		else
			%self.loadRotation = %rot;
		%params++;
	}
	if(%speed !$= "")
	{
		%self.loadSpeed = %speed;
		%params++;
	}
	if(%file !$= "")
	{
		if(isFile(%file) && fileExt(%file) $= ".bls")
		{
			%self.saveFile = %file;
			%params++;
		}
	}
	if(isObject(%bg))
	{
		%self.brickGroup = %bg;
		%params++;
	}
	if(%centre !$= "")
	{
		if(getWordCount(%centre) == 3)
		{
			%self.axis = %centre;
			%params++;
		}
	}
	if(%reset !$= "")
	{
		%self.reset = %reset;
		%params++;
	}
	if(%err !$= "")
	{
		%self.ignoreErrors = %err;
		%params++;
	}
	if(%params > 0)
		%self.loadParameters = true;
	return %params;
}

function BLSObject::loadStart(%self)
{
	if(%self.saveFile $= "")
	{
		error("BLSObject::loadStart - BLSObject instance has no saveFile attribute!");
		return;
	}
	%self.file = new FileObject(BLSObjectLoader);
	%self.file.openForRead(%self.saveFile);
	%self.currBrick = 0;
	%self.currIter = 0;
	%self.loaded = false;
	// %self.fetchLoadInformation();
	%self.loadTick();
}

function BLSObject::loadTick(%self)
{
	if(isEventPending(%self.loadTick))
		cancel(%self.loadTick);
	if(%self.loaded)
		return;
	%self.loadNext();
	%self.loadTick = %self.schedule(%self.loadSpeed, loadTick);
}

function BLSObject::loadComplete(%self)
{
	%self.loaded = true;
	%self.file.close();
	%self.file.delete();
	if(%self.reset)
	{
		%self.lastBrick = 0;
		%self.currBrick = 0;
		%self.brickCt = "";
		for(%i = 0; %i < %self.colors; %i++)
			%self.color[%i] = "";
		%self.colors = 0;
		%self.saveFile = "";
	}
}

function BLSObject::loadNext(%self)
{
	if(%self.loaded)
		return;
	if((%self.brickCt !$= "" && %self.currBrick > %self.brickCt) || %self.file.isEOF())
	{
		%self.loadComplete();
		return;
	}

	%line = %self.file.readLine();
	%quote = strPos(%line, "\"");
	if(%quote != -1)
	{
		%dbstring = getSubStr(%line, 0, %quote);
		%datablock = $UINameTable[%dbstring];
	}
	else
		%datablock = 0;
	if(firstWord(%line) $= "Linecount")
	{
		%self.brickCt = getWord(%line, 1);
		if(%self.brickCt <= 0)
		{
			%self.loadComplete();
			return;
		}
	}
	else if(isObject(%datablock) && %quote != -1)
	{
		%dbstring = getSubStr(%line, 0, %quote);
		%datablock = $UINameTable[%dbstring];
		if(!isObject(%datablock))
		{
			echo("whhat" SPC %dbstring);
			%self.currBrick++;
			return;
		}
		//echo(%datablock.getName());
		%values = strReplace(%line, %dbstring @ "\" ", "");
		// echo(%values);
		%position = getWords(%values, 0, 2);
		%angleID = getWord(%values, 3);
		%angleID += %self.loadRotation;
		if(%angleID > 3)
			%angleID -= 4;
		%position = rotateVector(%position, %self.axis, %self.loadRotation);
		%position = VectorAdd(%position, %self.loadOffset);
		%isBaseplate = getWord(values, 4);
		%colorID = %self.color[getWord(%values, 5)];
		%print = $PrintNameTable[getWord(%values, 6)];
		%colorFX = getWord(%values, 7);
		%shapeFX = getWord(%values, 8);
		%raycast = getWord(%values, 9);
		%collide = getWord(%values, 10);
		%renders = getWord(%values, 11);
		%client = 0;
		if(isObject(%self.brickGroup.client))
			%client = %self.brickGroup.client;
		%brick = new fxDTSBrick()
		{
			client = %client;
			stackBL_ID = %self.brickGroup.bl_id;

			datablock = %datablock;
			position = %position;
			rotation = angleToRot(%angleID);
			angleID = %angleID;
			colorID = %colorID;
			colorFXID = %colorFX;
			shapeFXID = %shapeFX;
			printID = %print;

			isBaseplate = %isBaseplate;
			isPlanted = true;
		};
		%val = %brick.plant();
		if(!%self.ignoreErrors && (!isObject(%brick) || %val > 0))
		{
			%brick.delete();
			%self.currBrick++;
			%self.lastBrick = 0;
			return;
		}

		%self.brickGroup.add(%brick);
		%brick.setTrusted(true);
		if(!%raycast)
			%brick.setRaycasting(false);
		if(!%collide)
			%brick.setColliding(false);
		if(!%renders)
			%brick.setRendering(false);

		%self.lastBrick = %brick;
		%self.currBrick++;
	}
	else if(isObject(%self.lastBrick))
	{
		if(!isObject(%self.lastBrick))
		{
			//%self.currBrick++;
			return;
		}
		%type = firstWord(%line);
		if(firstWord(%line) !$= "+-NTOBJECTNAME" && getField(%line, 0) !$= "+-EVENT" && %quote != -1)
			%uiName = restWords(getSubStr(%line, 0, %quote));
		switch$(%type)
		{
			case "+-NTOBJECTNAME": %self.lastBrick.setNTObjectName(restWords(%line));
			case "+-EMITTER":
				%equote = strPos(%line, "\" ") + 2;
				%direction = getSubStr(%line, %equote, strLen(%line));
				if(%direction - 2 >= 0)
					%direction += %self.loadRotation;
				if(%direction - 2 > 3)
					%direction -= 4;

				%self.lastBrick.emitterDirection = %direction;
				%self.lastBrick.setEmitter($UINameTable_Emitters[%uiName]);
			case "+-LIGHT": %self.lastBrick.setLight($UINameTable_Lights[%uiName]);
		}
	}
	else if(%self.currIter >= 3)
	{
		%self.color[%self.colors] = findClosestColor(%line);
		%self.colors++;
		%lastColor = %line;
	}
	%self.currIter++;
}

//some ripped from DRPG sorry :(
function SimSet::SaveCell(%this, %file)
{
	%path = "saves/" @ %file @ ".bls";
	if(!isWriteableFileName(%path))
	{
		echo("a");
		return false;
	}
	%file = new FileObject();
	%file.openForWrite(%path);
	%file.writeLine("This is a Blockland save file.  You probably shouldn't modify it cause you'll screw it up.");
	%file.writeLine(1);
	%file.writeLine("Dungeon Cell");
	for(%i = 0; %i < 64; %i++)
		%file.writeLine(getColorIDTable(%i));

	%file.writeLine("Linecount " @ %this.getCount());
	%ct = 0;
	for(%i = 0; %i < %this.getCount(); %i++)
	{
		%brick = %this.getObject(%i);
		
		if(%brick.getDataBlock().hasPrint)
		{
			%texture = getPrintTexture(%brick.getPrintId());
			%path = filePath(%texture);
			%underscorePos = strPos(%path, "_");
			%name = getSubStr(%path, %underscorePos + 1, strPos(%path, "_", 14) - 14) @ "/" @ fileBase(%texture);
			if($printNameTable[%name] !$= "")
			{
				%print = %name;
			}
		}

		%file.writeLine(%brick.getDataBlock().uiName @ "\" " @ %this.position[%brick] SPC %brick.getAngleID() SPC %brick.isBasePlate() SPC %brick.getColorID() SPC %print SPC %brick.getColorFXID() SPC %brick.getShapeFXID() SPC %brick.isRayCasting() SPC %brick.isColliding() SPC %brick.isRendering());

		if(isObject(%brick.emitter))
			%file.writeLine("+-EMITTER " @ %brick.emitter.emitter.uiName @ "\" " @ %brick.emitterDirection);
		if(%brick.getLightID() >= 0)
			%file.writeLine("+-LIGHT " @ %brick.getLightID().getDataBlock().uiName @ "\" "); // Not sure if something else comes after the name
		%ct++;
	}
	%file.close();
	%file.delete();
	return %ct;
}

function fxDTSBrick::CreateCellGroup(%this)
{
	%group = new SimSet();
	%group.add(%this);
	%pos = "0 0 0";
	%basePos = VectorSub(%this.getPosition(), "0 0 0.1");
	%group.position[%this] = %pos;
	%iter = 1;
	for(%i = 0; %i != %iter; %i++)
	{
		%brick = %group.getObject(%i);
		if(!isObject(%brick))
			continue;
		%up = %brick.getNumUpBricks();
		if(%i > 0)
		{
			%down = %brick.getNumDownBricks();
			for(%b = 0; %b < %down; %b++)
			{
				%obj = %brick.getDownBrick(%b);
				if(!isObject(%obj) || %group.isMember(%obj))
					continue;
				%pos = VectorSub(%obj.getPosition(), %basePos); //uhh
				%group.add(%obj);
				%group.position[%obj] = %pos;
				%iter++;
			}
		}
		for(%b = 0; %b < %up; %b++)
		{
			%obj = %brick.getUpBrick(%b);
			if(!isObject(%obj) || %group.isMember(%obj))
				continue;
			%pos = VectorSub(%obj.getPosition(), %basePos); //uhh
			%group.add(%obj);
			%group.position[%obj] = %pos;
			%iter++;
		}
	}
	return %group;
}

function serverCmdCaptureCell(%this)
{
	if(!isObject(%this.player))
	{
		messageClient(%this, '', "You must have a player object!");
		return;
	}
	if(!(%this.isAdmin || %this.isSuperAdmin))
	{
		messageClient(%this, '', "You must be admin.");
		return;
	}
	if(!isObject(%this.player.tempBrick))
	{
		messageClient(%this, '', "Move your ghost brick to encompass the base of the cell.");
		return;
	}
	if($Sim::Time - %this.lastCellCap < 1)
	{
		messageClient(%this, '', "\Slow down!");
		return;
	}

	initContainerBoxSearch(%this.player.tempBrick.getPosition(), "0 0 0", $TypeMasks::FxBrickObjectType);
	%brick = containerSearchNext();
	if(!isObject(%brick))
	{
		messageClient(%this, '', "Move your ghost brick to encompass the base of the cell.");
		return;
	}
	%group = %brick.CreateCellGroup();
	for(%i = 0; %i < %group.getCount(); %i++)
	{
		%b = %group.getObject(%i);
		%b.ofx = %b.getColorFXID();
		%b.setColorFX(3);
		%b.ofxset = %b.schedule(1500, setColorFX, %b.ofx);
	}
	%this.capturedCell = %group;
	%this.lastCellCap = $Sim::Time;
	messageClient(%this, '', "Captured" SPC %group.getCount() SPC "bricks.");
}

function serverCmdSaveCell(%this, %layout, %cell, %type)
{
	if(!(%this.isAdmin || %this.isSuperAdmin))
	{
		messageClient(%this, '', "You must be admin.");
		return;
	}
	if(!isObject(%this.capturedCell))
	{
		messageClient(%this, '', "Capture a cell using /captureCell first!");
		return;
	}
	if(findWord($DungeonCellTypes, %cell) == -1)
	{
		messageClient(%this, '', "This cell type doesn't exist! Do /cellTypes to see the list. (For decor, do /saveCell style decor num");
		return;
	}
	if(%type !$= "")
		%cell = %cell @ %type;
	%file = "dungeon_" @ %layout @ "_" @ %cell;
	if(isFile("saves/" @ %file @ ".bls"))
	{
		if(!%this.isSuperAdmin)
		{
			messageClient(%this, '', "Cell already exists, only SA can overwrite!");
			return;
		}
		else if(!%this.cellConfirm[%layout, %cell, %type] && %this.lastConfirm !$= %layout TAB %cell TAB %type)
		{
			messageClient(%this, '', "This cell piece already exists. Do this command again to confirm overwrite.");
			%this.cellConfirm[%layout, %cell, %type] = true;
			%this.lastconfirm = %layout TAB %cell TAB %type;
			return;
		}
		else if(%this.cellConfirm[%layout, %cell, %type])
			%this.cellConfirm[%layout, %cell, %type] = false;
	}
	%s = %this.capturedCell.saveCell(%file);
	if(!%s)
	{
		messageClient(%this, '', "Something went wrong!");
		return;
	}
	messageClient(%this, '', "\c2Saved" SPC %s SPC "bricks to" SPC %file @ "!");
	%this.capturedCell = 0;
	if(%this.lastConfirm !$= "")
	{
		%this.cellConfirm[getField(%this.lastConfirm, 0), getField(%this.lastConfirm, 1), getField(%this.lastConfirm, 2)] = false;
		%this.lastConfirm = "";
	}
}

function serverCmdCellTypes(%this)
{
	%ct = getWordCount($DungeonCellTypes);
	for(%i = 0; %i < %ct; %i++)
		messageClient(%this, '', strUpr(getWord($DungeonCellTypes, %i)));
}