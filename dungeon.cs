$Dungeon::Keys = -1;
$Dungeon::Keys[$Dungeon::Keys++] = "BLANK";
$Dungeon::Keys[$Dungeon::Keys++] = "FLOOR";
$Dungeon::Keys[$Dungeon::Keys++] = "CORNER";
$Dungeon::Keys[$Dungeon::Keys++] = "WALL";
$Dungeon::Keys[$Dungeon::Keys++] = "WALL";
$Dungeon::Keys[$Dungeon::Keys++] = "WALL";
$Dungeon::Keys[$Dungeon::Keys++] = "WALL";
$Dungeon::Keys[$Dungeon::Keys++] = "DOOR";
$Dungeon::Keys[$Dungeon::Keys++] = "SUP";
$Dungeon::Keys[$Dungeon::Keys++] = "SDOWN";
$Dungeon::Keys[$Dungeon::Keys++] = "DECOR";

$Dungeon::Buildable = "floor corner wall door sup sdown decor";
$Dungeon::NumDefaultfloor = 0;
$Dungeon::NumDefaultcorner = 0;
$Dungeon::NumDefaultwall = 0;
$Dungeon::NumDefaultdoor = 0;
$Dungeon::NumDefaultsup = 0;
$Dungeon::NumDefaultsdown = 0;
$Dungeon::NumDefaultdecor = 0;

$Dungeon::Print0 = "+";
$Dungeon::Print1 = " ";
$Dungeon::Print2 = "#";
$Dungeon::Print3 = "^";
$Dungeon::Print4 = ">";
$Dungeon::Print5 = "v";
$Dungeon::Print6 = "<";
$Dungeon::Print7 = "|";
$Dungeon::Print8 = "&";
$Dungeon::Print9 = "%";

$Dungeon::BPColor0 = 8;
$Dungeon::BPColor1 = 62;
$Dungeon::BPColor2 = 59;
$Dungeon::BPColor3 = 58;
$Dungeon::BPColor4 = 57;
$Dungeon::BPColor5 = 56;
$Dungeon::BPColor6 = 55;
$Dungeon::BPColor7 = 61;
$Dungeon::BPColor8 = 2;
$Dungeon::BPColor9 = 4;

$Dungeon::DirDict3 = "NORTH";
$Dungeon::DirDict4 = "EAST";
$Dungeon::DirDict5 = "SOUTH";
$Dungeon::DirDict6 = "WEST";

$Dungeon::SavesDir = "saves/";
$Dungeon::Types = "";
$Dungeon::DefaultType = "halls";

$Dungeon::Tutorial = 	"\c5--\c6DUNGEON CELL CREATION\c5--" NL
						"\c5Say \c3/dungeonBuild \c4[num] \c5for information on a specific topic." NL
						"\c31. \c6What you're doing" NL
						"\c32. \c6Building a cell" NL
						"\c33. \c6Notes" NL
						"\c34. \c6When you're done";

$Dungeon::Tutorial1 = 	"\c5----------" NL
						"\c6The dungeon generator's job is only to create the information about the dungeon." NL
						"\c6To actually build the dungeon in 3D space, saves that represent this information are needed." NL
						"\c6These saves are referred to as \c3cells\c6. These are what you are building." NL
						"\c6When building the dungeon, the generator loads the save of the cell it needs from the style of dungeon it's set to use with correct rotation and placement." NL
						"\c6Keep in mind that your set of cells will be repeated. Make sure they don't look strange looped when building." NL
						"\c5----------";

$Dungeon::Tutorial2 =	"\c5----------" NL
						"\c6Building a cell is as simple as making the element of the dungeon in question in your own style." NL
						"\c6There is no height limit on cells, and the baseplates can be any size as long as you remain consistent." NL
						"\c6Generally it's good to have a reference for what direction cells need to face, as this matters for things like walls and corners." NL
						"\c6For a list of cell types, say \c3/cellTypes\c6." NL
						"\c6You can see the existing styles by saying \c3/listStyles\c6." NL
						"\c5----------";

$Dungeon::Tutorial3 =	"\c5----------" NL
						"\c6Orientation is extremely important. Make certain that your cells face the same way as cells in known working styles." NL
						"\c6This is something the builder can't guess, and if it's wrong, that cell type will be incorrectly rotated in the end." NL
						"\c6You can have multiple builds of the same kind of cell. The generator will simply choose a random type. This can be useful for giving your style variety." NL
						"\c6Decorations randomly replace floor tiles. Keep this in mind while building them." NL
						"\c6It's good to be conservative, but don't let that discourage you from being detailed." NL
						"\c6As far as script is concerned, stairs up is the entrance for the dungeon, and stairs down is the exit." NL
						"\c6If you want it to seem like the player is going upward in your style, you can make up look like down and down look like up. The generator doesn't care." NL
						"\c6The builder does not load events." NL
						"\c5----------";

$Dungeon::Tutorial4 =	"\c5----------" NL
						"\c6When you've finished a style, get ahold of an admin (probably me) and if what you've built will work, they will save your style." NL
						"\c6It takes a bit of manual work to implement them into the script, so be patient. I usually test styles almost immediately after I add them, though." NL
						"\c6It's fine to make changes to your style after they're already added; applying the changes is very simple. If you feel your style could be better, by all means, improve it." NL
						"\c5----------";

function serverCmdDungeonBuild(%this, %category)
{
	if(%category > 0 && %category < 5)
	{
		%r = getRecordCount($Dungeon::Tutorial[%category]);
		for(%i = 0; %i < %r; %i++)
			messageClient(%this, '', getRecord($Dungeon::Tutorial[%category], %i));
	}
	else
	{
		%r = getRecordCount($Dungeon::Tutorial);
		for(%i = 0; %i < %r; %i++)
			messageClient(%this, '', getRecord($Dungeon::Tutorial, %i));
	}
}

function serverCmdListStyles(%this)
{
	%ct = getWordCount($Dungeon::Types);
	for(%i = 0; %i < %ct; %i++)
		messageClient(%this, '', getWord($Dungeon::Types, %i));
}

function Dist2D(%p1, %p2)
{
	%x1 = getWord(%p1, 0);
	%y1 = getWord(%p1, 1);
	%x2 = getWord(%p2, 0);
	%y2 = getWord(%p2, 1);

	%r1 = mPow(%x2 - %x1, 2);
	%r2 = mPow(%y2 - %y1, 2);

	return mSqrt(%r1 + %r2);
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

function Dungeon_addStyle(%name, %size)
{
	if(findWord($Dungeon::Types, %name) != -1)
	{
		warn("Style" SPC %name SPC "already added, skipping...");
		return;
	}
	$Dungeon::LayoutCellSize[%name] = %size;
	// $Dungeon::LayoutLightHeight[%name] = 5.2;
	// $Dungeon::LayoutLightDatablock[%name] = (isObject(DimAmbientLight) ? DimAmbientLight : PlayerLight);
	%patternPre = $Dungeon::SavesDir @ "dungeon_" @ %name @ "_";
	%ct = getWordCount($Dungeon::Buildable);
	for(%i = 0; %i < %ct; %i++)
	{
		%type = getWord($Dungeon::Buildable, %i);
		$Dungeon::Layout[%type @ "Num" @ %name] = $Dungeon::NumDefault[%type];
		%pattern = %patternPre @ %type @ "*.bls";
		for(%f = findFirstFile(%pattern); isFile(%f); %f = findNextFile(%pattern))
			$Dungeon::Layout[%type @ "Num" @ %name]++;
		if($Dungeon::Layout[%type @ "Num" @ %name] < $Dungeon::NumDefault[%type])
			$Dungeon::Layout[%type @ "Num" @ %name] = 0;
	}
	$Dungeon::Types = trim($Dungeon::Types SPC %name);
	echo("Added the" SPC %name SPC "dungeon style...");
}

function Dungeon_loadDefaultStyles()
{
	//By dwaif
	Dungeon_addStyle("stone", 8);
	// $Dungeon::LayoutCellSizestone = 8;
	// $Dungeon::LayoutLightHeightstone = 5.2;
	// $Dungeon::LayoutLightDatablockstone = (isObject(DimAmbientLight) ? DimAmbientLight : PlayerLight);
	// $Dungeon::LayoutDecorNumstone = 0;
	// $Dungeon::LayoutFloorNumstone = 1;
	//By ottosparks
	Dungeon_addStyle("mine", 8);
	// $Dungeon::LayoutCellSizemine = 8;
	// $Dungeon::LayoutLightHeightmine = 5.2;
	// $Dungeon::LayoutLightDatablockmine = (isObject(DimAmbientLight) ? DimAmbientLight : PlayerLight);
	// $Dungeon::LayoutDecorNummine = 0;
	//By DarkStride
	Dungeon_addStyle("temple", 16);
	// $Dungeon::LayoutCellSizetemple = 16;
	// $Dungeon::LayoutLightHeighttemple = 5.2;
	// $Dungeon::LayoutLightDatablocktemple = (isObject(DimAmbientLight) ? DimAmbientLight : PlayerLight);
	// $Dungeon::LayoutDecorNumtemple = 0;
	//By Mr. Nobody
	Dungeon_addStyle("halls", 8);
	// $Dungeon::LayoutCellSizehalls = 8;
	// $Dungeon::LayoutLightHeighthalls = 5.2;
	// $Dungeon::LayoutLightDatablockhalls = (isObject(DimAmbientLight) ? DimAmbientLight : PlayerLight);
	// $Dungeon::LayoutDecorNumhalls = 4;
	//By Alphadin
	Dungeon_addStyle("scifi", 8);
	// $Dungeon::LayoutCellSizescifi = 8;
	// $Dungeon::LayoutLightHeightscifi = 5.2;
	// $Dungeon::LayoutLightDatablockscifi = (isObject(DimAmbientLight) ? DimAmbientLight : PlayerLight);
	// $Dungeon::LayoutDecorNumscifi = 0;
	//By Armageddon
	Dungeon_addStyle("cathedral", 8);
	// $Dungeon::LayoutCellSizecathedral = 8;
	// $Dungeon::LayoutLightHeightcathedral = 5.2;
	// $Dungeon::LayoutLightDatablockcathedral = (isObject(DimAmbientLight) ? DimAmbientLight : PlayerLight);
	// $Dungeon::LayoutDecorNumcathedral = 3;
	//By Mr. Nobody
	Dungeon_addStyle("powercore", 8);
	// $Dungeon::LayoutCellSizepowercore = 8;
	// $Dungeon::LayoutLightHeightpowercore = 5.2;
	// $Dungeon::LayoutLightDatablockpowercore = (isObject(DimAmbientLight) ? DimAmbientLight : PlayerLight);
	// $Dungeon::LayoutDecorNumpowercore = 2;
	//By Flamecannon
	Dungeon_addStyle("worldone", 16);
	//By Bokeh, Armageddon, and Morgellons
	Dungeon_addStyle("sewer", 8);
	//By Kalen
	Dungeon_addStyle("lunaris", 8);wwwww
}

//this is so i know the rotations of the wall pieces ingame
//		2
//		^
//		N
//3<--W-#-E-->1
//		S
//		v
//		0	

function newDungeon(%name, %sx, %sy, %maxRooms, %rminX, %rminY, %rmaxX, %rmaxY, %style, %decorDiv)
{
	if(isObject("Dungeon_" @ %name))
	{
		error("newDungeon - Dungeon of this name already exists! (" @ %name @ ")");
		return;
	}

	%this = new ScriptObject("Dungeon_" @ %name)
	{
		class = "Dungeon";
		name = %name;
		style = %style;

		sizeX = %sx;
		sizeY = %sy;

		rooms = 0;
		maxRooms = %maxRooms;
		roomMinX = %rminX;
		roomMinY = %rminY;
		roomMaxX = %rmaxX;
		roomMaxY = %rmaxY;
		rDist = Dist2D(%rminX SPC %rminY, %rmaxX SPC %rmaxy);

		loadOffset = "0 0 0";
		decorDiv = (%decorDiv > 0 ? %decorDiv : 5);
	};
	return %this;
}

function newRoom(%sx, %sy)
{
	%this = new ScriptObject("DungeonRoom")
	{
		posX = 0;
		posY = 0;

		sizeX = %sx;
		sizeY = %sy;
	};
	return %this;
}

function Dungeon::addRoom(%self, %room)
{
	if(%room.getName() !$= "DungeonRoom")
		return;
	%self.room[%self.rooms] = %room;
	%self.rooms++;
}

function Dungeon::genRoom(%self, %minx, %miny, %maxx, %maxy)
{
	%sizeX = getRandom(%minx, %maxx);
	%sizeY = getRandom(%miny, %maxy);
	%room = newRoom(%sizeX, %sizeY);

	for(%y = 0; %y < %sizeY; %y++)
	{
		for(%x = 0; %x < %sizeX; %x++)
		{
			if(%x == 0 && %y == 0)
				%room.tiles[%x, %y] = 2;
			else if(%x == %sizeX - 1 && %y == 0)
				%room.tiles[%x, %y] = 2;
			else if(%x == 0 && %y == %sizeY - 1)
				%room.tiles[%x, %y] = 2;
			else if(%x == %sizeX - 1 && %y == %sizeY - 1)
				%room.tiles[%x, %y] = 2;
			else if(%y == 0)
				%room.tiles[%x, %y] = 3;
			else if(%x == %sizeX - 1)
				%room.tiles[%x, %y] = 4;
			else if(%y == %sizeY - 1)
				%room.tiles[%x, %y] = 5;
			else if(%x == 0)
				%room.tiles[%x, %y] = 6;
			else
				%room.tiles[%x, %y] = 1;
		}
	}

	return %room;
}

function Dungeon::getBranchingPos(%self)
{
	%branchingRoom = %self.room[getRandom(0, %self.rooms)];
	%btpX = 0;
	%btpY = 0;
	for(%i = 0; %i < %branchingRoom.sizeX * %branchingRoom.sizeY; %i++)
	{
		%x = getRandom(%branchingRoom.sizeX, %branchingRoom.posX + %branchingRoom.sizeX - 1);
		%y = getRandom(%branchingRoom.sizeY, %branchingRoom.posY + %branchingRoom.sizeY - 1);

		if(%self.grid[%x, %y] > 2)
		{
			%btpX = %x;
			%btpY = %y;
			break;
		}
	}
	return %btpX SPC %btpY;
}

function Dungeon::getBranchingDir(%self, %bpX, %bpY)
{
	%dir = $Dungeon::DirDict[%self.grid[%bpX, %bpY]];
	if(%dir $= "")
		return "";
	return %dir;
}

function Dungeon::checkRoom(%self, %sX, %sY, %pX, %pY)
{
	for(%y = %pY; %y < %pY + %sY; %y++)
	{
		for(%x = %pX; %x < %pX + %sX; %x++)
		{
			if(%x < 0 || %x > %self.sizeX - 1)
				return false;
			if(%y < 0 || %y > %self.sizeY - 1)
				return false;
			if(%self.grid[%x, %y] != 0)
				return false;
		}
	}
	return true;
}

function Dungeon::placeRoom(%self, %room, %gX, %gY)
{
	%room.posX = %gX;
	%room.posY = %gY;

	%rtX = 0;
	%rtY = 0;
	for(%y = %gY; %y < %gY + %room.sizeY; %y++)
	{
		for(%x = %gX; %x < %gX + %room.sizeX; %x++)
		{
			if(%room.tiles[%rtX, %rtY] == 2)
				%self.rotation[%x, %y] = %room.solveCornerRot(%rtX, %rtY);
			else if($Dungeon::DirDict[%room.tiles[%rtX, %rtY]] !$= "")
			{
				switch(%room.tiles[%rtX, %rtY])
				{
					case 3: %self.rotation[%x, %y] = 2;
					case 4: %self.rotation[%x, %y] = 1;
					case 5: %self.rotation[%x, %y] = 0;
					case 6: %self.rotation[%x, %y] = 3;
				}
			}
			%self.grid[%x, %y] = %room.tiles[%rtX, %rtY];
			switch(%self.grid[%x, %y])
			{
				case 1:
					if($Dungeon::LayoutFloorNum[%self.style] != $Dungeon::NumDefaultfloor)
						%self.type[%x, %y] = getRandom(1, $Dungeon::LayoutFloorNum[%self.style]);
					%self.rotation[%x, %y] = getRandom(0, 3);
				case 2:
					if($Dungeon::LayoutcornerNum[%self.style] != $Dungeon::NumDefaultcorner)
						%self.type[%x, %y] = getRandom(1, $Dungeon::LayoutcornerNum[%self.style]);
				case 3:
					if($Dungeon::LayoutwallNum[%self.style] != $Dungeon::NumDefaultwall)
						%self.type[%x, %y] = getRandom(1, $Dungeon::LayoutwallNum[%self.style]);
				case 4:
					if($Dungeon::LayoutwallNum[%self.style] != $Dungeon::NumDefaultwall)
						%self.type[%x, %y] = getRandom(1, $Dungeon::LayoutwallNum[%self.style]);
				case 5:
					if($Dungeon::LayoutwallNum[%self.style] != $Dungeon::NumDefaultwall)
						%self.type[%x, %y] = getRandom(1, $Dungeon::LayoutwallNum[%self.style]);
				case 6:
					if($Dungeon::LayoutwallNum[%self.style] != $Dungeon::NumDefaultwall)
						%self.type[%x, %y] = getRandom(1, $Dungeon::LayoutwallNum[%self.style]);
				case 7:
					if($Dungeon::LayoutdoorNum[%self.style] != $Dungeon::NumDefaultdoor)
						%self.type[%x, %y] = getRandom(1, $Dungeon::LayoutdoorNum[%self.style]);
			}
			//echo(%room.tiles[%rtX, %rtY] SPC %self.grid[%x, %y]);
			%rtX++;
		}
		%rtY++;
		%rtX = 0;
	}
}

function Dungeon::connectRooms(%self, %bpX, %bpY, %dir)
{
	%self.grid[%bpX, %bpY] = 7;
	switch$(%dir)
	{
		case "NORTH":	%self.grid[%bpX, %bpY-1] = 7;
						%self.rotation[%bpX, %bpY] = 2;
						%self.rotation[%bpX, %bpY - 1] = 0;
		case "EAST":	%self.grid[%bpX+1, %bpY] = 7;
						%self.rotation[%bpX, %bpY] = 1;
						%self.rotation[%bpX + 1, %bpY] = 3;		
		case "SOUTH":	%self.grid[%bpX, %bpY+1] = 7;
						%self.rotation[%bpX, %bpY] = 0;
						%self.rotation[%bpX, %bpY + 1] = 2;
		case "WEST":	%self.grid[%bpX-1, %bpY] = 7;
						%self.rotation[%bpX, %bpY] = 3;
						%self.rotation[%bpX - 1, %bpY] = 1;
	}
}

function Dungeon::setStairs(%self)
{
	for(%i = 0; %i < %self.rooms; %i++)
	{
		%room = %self.room[getRandom(0, %self.rooms)];
		%x = %room.posX + mFloatLength(%room.sizeX / 2, 0);
		%y = %room.posY + mFloatLength(%room.sizeY / 2, 0);
		if(%self.grid[%x, %y] == 1)
		{
			%self.supRoom = %room;
			%self.grid[%x, %y] = 8;
			%self.rotation[%x, %y] = getRandom(0, 3);
			if($Dungeon::LayoutsupNum[%self.style] != $Dungeon::NumDefaultsup)
				%self.type[%x, %y] = getRandom(1, $Dungeon::LayoutsupNum[%self.style]);
			break;
		}
	}

	for(%i = 0; %i < %self.rooms; %i++)
	{
		%room = %self.room[getRandom(0, %self.rooms)];
		%x = %room.posX + mFloor(%room.sizeX / 2);
		%y = %room.posY + mFloor(%room.sizeY / 2);
		if(Dist2D(%room.posX SPC %room.posY, %self.supRoom.posX SPC %self.supRoom.posY) < %self.rDist * 2)
			continue;
		if(%self.grid[%x, %y] == 1 && (%room != %self.supRoom && %self.rooms > 1))
		{
			%self.sdownRoom = %room;
			%self.grid[%x, %y] = 9;
			%self.rotation[%x, %y] = getRandom(0, 3);
			if($Dungeon::LayoutsdownNum[%self.style] != $Dungeon::NumDefaultsdown)
				%self.type[%x, %y] = getRandom(1, $Dungeon::LayoutsdownNum[%self.style]);
			break;
		}
	}
	if(!isObject(%self.sdownRoom))
	{
		for(%i = 0; %i < %self.rooms; %i++)
		{
			%room = %self.room[getRandom(0, %self.rooms)];
			%x = %room.posX + mFloor(%room.sizeX / 2);
			%y = %room.posY + mFloor(%room.sizeY / 2);
			if(%self.grid[%x, %y] == 1)
			{
				%self.sdownRoom = %room;
				%self.grid[%x, %y] = 9;
				%self.rotation[%x, %y] = getRandom(0, 3);
				if($Dungeon::LayoutsdownNum[%self.style] != $Dungeon::NumDefaultsdown)
					%self.type[%x, %y] = getRandom(1, $Dungeon::LayoutsdownNum[%self.style]);
				break;
			}
		}
	}
	echo("Set stairs; generating decorations...");
	%self.schedule(%self.len, setDecorations);
}

function Dungeon::setDecorations(%self)
{
	if($Dungeon::LayoutDecorNum[%self.style] <= 0)
	{
		echo("Skipped decorations...");
		echo("Done.");
		return;
	}
	for(%i = 0; %i < %self.rooms; %i++)
	{
		%room = %self.room[%i];
		%x = %room.sizeX;
		%y = %room.sizeY;
		%area = %x * %y;
		%hArea = mFloatLength(%area / 2, 0);
		%maxDecor = mFloatLength(%area / %self.decorDiv, 0);
		for(%d = 0; %d < %maxDecor; %d++)
		{
			%r = getRandom(1, %area);
			if(%r > %hArea)
				continue;
			%pX = %room.posX + getRandom(1, %x);
			%pY = %room.posY + getRandom(1, %y);
			if(%self.grid[%pX, %pY] != 1)
				continue;
			%decor = getRandom(1, $Dungeon::LayoutDecorNum[%self.style]);
			%self.grid[%pX, %pY] = 10;
			%self.type[%pX, %pY] = %decor;
			%self.rotation[%pX, %pY] = getRandom(0, 3);
		}
	}
	echo("Done.");
}

function Dungeon::startRooms(%self, %area)
{
	%self.currIter = 0;
	%self.area = %area;
	%self.done = false;
}

function Dungeon::nextRoom(%self)
{
	if(%self.done || %self.currIter >= %self.area)
	{
		%self.roomsDone();
		return;
	}
	%i = %self.currIter;
	if(%self.maxRooms != 0)
	{
		if(%self.rooms == %self.maxRooms)
		{
			%self.roomsDone();
			return;
		}
	}

	%bPos = %self.getBranchingPos();
	%bpX = getWord(%bPos, 0);
	%bpY = getWord(%bPos, 1);
	%dir = %self.getBranchingDir(%bpX, %bpY);
	if(%dir !$= "")
	{
		%nPosX = 0;
		%nPosY = 0;
		%room = %self.genRoom(%self.roomMinX, %self.roomMinY, %self.roomMaxX, %self.roomMaxY);

		switch$(%dir)
		{
			case "NORTH":
				%nPosX = (%bpX - mFloor(%room.sizeX / 2));
				%nPosY = (%bpY - %room.sizeY);
			case "EAST":
				%nPosX = (%bpX + 1);
				%nPosY = (%bpY - mFloor(%room.sizeY / 2));
			case "SOUTH":
				%nPosX = (%bpX - mFloor(%room.sizeX / 2));
				%nPosY = (%bpY + 1);
			case "WEST":
				%nPosX = (%bpX - %room.sizeX);
				%nPosY = (%bpY - mFloor(%room.sizeY / 2));
		}
		if(%self.checkRoom(%room.sizeX, %room.sizeY, %nPosX, %nPosY))
		{
			%self.placeRoom(%room, %nPosX, %nPosY);
			%self.addRoom(%room);
			%self.connectRooms(%bpX, %bpY, %dir);
		}
		else
		{
			%room.delete();
			%self.currIter++;
		}
	}
}

function Dungeon::roomsDone(%self)
{
	%self.done = true;
	talk("rooms done");
}

function Dungeon::buildLoop(%self)
{
	if(isEventPending(%self.loop))
		cancel(%self.loop);
	if(%self.done)
	{
		echo("Generated map; setting staircases...");
		%self.setStairs();
		return;
	}
	%self.nextRoom();
	%self.loop = %self.schedule(%self.len, buildLoop);
}

function Dungeon::genMap(%self)
{
	for(%y = 0; %y < %self.sizeY; %y++)
	{
		for(%x = 0; %x < %self.sizeX; %x++)
			%self.grid[%x, %y] = 0;
	}

	%self.addRoom(%self.genRoom(%self.roomMinX, %self.roomMinY, %self.roomMaxX, %self.roomMaxY));
	%self.placeRoom(%self.room0, mFloor(%self.sizeX / 2) - mFloor(%self.room0.sizeX / 2), mFloor(%self.sizeY / 2) - mFloor(%self.room0.sizeY / 2));

	%area = ((%self.sizeX * %self.sizeY) * 2);
	%self.startRooms(%area);
	%self.buildLoop();
	// for(%i = 0; %i < %area; %i++)
	// {
	// 	if(%self.maxRooms != 0)
	// 	{
	// 		if(%self.rooms == %self.maxRooms)
	// 			break;
	// 	}

	// 	%bPos = %self.getBranchingPos();
	// 	%bpX = getWord(%bPos, 0);
	// 	%bpY = getWord(%bPos, 1);
	// 	%dir = %self.getBranchingDir(%bpX, %bpY);
	// 	if(%dir !$= "")
	// 	{
	// 		%nPosX = 0;
	// 		%nPosY = 0;
	// 		%room = %self.genRoom(%self.roomMinX, %self.roomMinY, %self.roomMaxX, %self.roomMaxY);

	// 		switch$(%dir)
	// 		{
	// 			case "NORTH":
	// 				%nPosX = (%bpX - mFloor(%room.sizeX / 2));
	// 				%nPosY = (%bpY - %room.sizeY);
	// 			case "EAST":
	// 				%nPosX = (%bpX + 1);
	// 				%nPosY = (%bpY - mFloor(%room.sizeY / 2));
	// 			case "SOUTH":
	// 				%nPosX = (%bpX - mFloor(%room.sizeX / 2));
	// 				%nPosY = (%bpY + 1);
	// 			case "WEST":
	// 				%nPosX = (%bpX - %room.sizeX);
	// 				%nPosY = (%bpY - mFloor(%room.sizeY / 2));
	// 		}
	// 		if(%self.checkRoom(%room.sizeX, %room.sizeY, %nPosX, %nPosY))
	// 		{
	// 			%self.placeRoom(%room, %nPosX, %nPosY);
	// 			%self.addRoom(%room);
	// 			%self.connectRooms(%bpX, %bpY, %dir);
	// 		}
	// 		else
	// 			%i++;
	// 	}
	// }
}

function Dungeon::generate(%self)
{
	echo("Generating dungeon...");
	%self.genMap();
	// echo("Generated map; setting staircases...");
	// %self.setStairs();
	// echo("Done.");
	return %self;
}

function Dungeon::destroy(%self)
{
	if(%self.rooms > 0)
	{
		for(%i = 0; %i < %self.rooms; %i++)
			%self.room[%i].delete();
	}
	if(isObject(%self.blsParser))
		%self.blsParser.delete();
	%self.delete();
}

// function Dungeon::solveCornerRot(%self, %x, %y)
// {
// 	if(%self.grid[%x, %y] != 2)
// 		return 0;
// 	%north = %self.grid[%x+1, %y];
// 	%south = %self.grid[%x-1, %y];
// 	%east = %self.grid[%x, %y+1];
// 	%west = %self.grid[%x, %y-1];
// 	// echo(%north SPC %south SPC %east SPC %west);
// 	if(%north == 3 && %east == 6)
// 		return 3;
// 	if(%north == 5 && %west == 6)
// 		return 0;
// 	if(%south == 5 && %west == 4)
// 		return 1;
// 	if(%south == 3 && %east == 4)
// 		return 2;
// 	return 0;
// }

function DungeonRoom::solveCornerRot(%self, %x, %y)
{
	if(%self.tiles[%x, %y] != 2)
		return 0;
	%north = %self.tiles[%x+1, %y];
	%south = %self.tiles[%x-1, %y];
	%east = %self.tiles[%x, %y+1];
	%west = %self.tiles[%x, %y-1];
	// echo(%north SPC %south SPC %east SPC %west);
	if(%north == 3 && %east == 6)
		return 3;
	if(%north == 5 && %west == 6)
		return 0;
	if(%south == 5 && %west == 4)
		return 1;
	if(%south == 3 && %east == 4)
		return 2;
	return 0;
}

function Dungeon::solveDoorRot(%self, %x, %y)
{
	if(%self.grid[%x, %y] != 7)
		return 0;
	%north = %self.grid[%x+1, %y];
	%south = %self.grid[%x-1, %y];
	%east = %self.grid[%x, %y+1];
	%west = %self.grid[%x, %y-1];
	if(%east == %west)
		return 0;
	if(%north == %south)
		return 1;
	return 0;
}

function Dungeon::print(%self)
{
	for(%x = 0; %x < %self.sizeX; %x++)
	{
		for(%y = 0; %y < %self.sizeY; %y++)
			%str = %str @ $Dungeon::Print[%self.grid[%x, %y]];
		echo(%str);
		%str = "";
	}
}

function Dungeon::blueprintStart(%self)
{
	%self.cX = 0;
	%self.cY = 0;
	%self.bpDone = false;
}

function Dungeon::blueprintNext(%self)
{
	if(%self.bpDone)
		return;
	if(%self.cY >= %self.sizeY)
	{
		%self.cX++;
		%self.cY = 0;
		for(%i = 0; %i < %self.sizeY; %i++)
			commandToServer('superShiftBrick', 0, 1, 0);
		commandToServer('superShiftBrick', 1, 0, 0);
	}
	if(%self.cX >= %self.sizeX)
	{
		%self.blueprintDone();
		return;
	}
	commandToServer('useSprayCan', $Dungeon::BPColor[%self.grid[%self.cX, %self.cY]]);
	commandToServer('plantBrick');
	commandToServer('superShiftBrick', 0, -1, 0);
	%self.cY++;
}

function Dungeon::blueprintDone(%self)
{
	%self.bpDone = true;
}

function Dungeon::blueprintLoop(%self, %s)
{
	if(isEventPending(%self.bpLoop))
		cancel(%self.bpLoop);
	if(%self.bpDone)
		return;
	%self.blueprintNext();
	self.bpLoop = %self.schedule(%s, blueprintLoop, %s);
}

function Dungeon::buildBlueprint(%self, %s)
{
	%self.blueprintStart();
	%self.blueprintLoop(%s);
	// %self.buildQueue = new ScriptObject(){sc=-1;};
	// for(%x = 0; %x < %self.sizeX; %x++)
	// {
	// 	for(%y = 0; %y < %self.sizeY; %y++)
	// 	{
	// 		%self.buildQueue.s[%self.buildQueue.sc++] = schedule(%self.buildQueue.sc * %s, 0, commandToServer, 'useSprayCan', $Dungeon::BPColor[%self.grid[%x, %y]]);
	// 		%self.buildQueue.s[%self.buildQueue.sc++] = schedule(%self.buildQueue.sc * %s, 0, commandToServer, 'plantBrick');
	// 		%self.buildQueue.s[%self.buildQueue.sc++] = schedule(%self.buildQueue.sc * %s, 0, commandToServer, 'superShiftBrick', 0, -1, 0);
	// 	}
	// 	for(%i = 0; %i < %self.sizeY; %i++)
	// 		%self.buildQueue.s[%self.buildQueue.sc++] = schedule(%self.buildQueue.sc * %s, 0, commandToServer, 'superShiftBrick', 0, 1, 0);
	// 	%self.buildQueue.s[%self.buildQueue.sc++] = schedule(%self.buildQueue.sc * %s, 0, commandToServer, 'superShiftBrick', 1, 0, 0);
	// }
	// %self.buildQueue.s[%self.buildQueue.sc++] = %self.buildQueue.schedule(%self.buildQueue.sc * %s + 50, delete);
}

function Dungeon::loadCells(%self, %speed)
{
	if(%speed $= "")
		%speed = 0;
	if(!isObject(%self.blsParser))
		%self.blsParser = newBLSObject();
	%self.blsParser.reset = true;
	%self.loadSpeed = %speed;
	%self.loadStart();
}

function Dungeon::loadStart(%self)
{
	%self.clX = 0;
	%self.clY = 0;
	%self.loadDone = false;
	%self.cellLoaded();
}

function Dungeon::cellLoaded(%self)
{
	if(%self.loadDone)
		return;
	if(%self.clY >= %self.sizeY)
	{
		%self.clX++;
		%self.clY = 0;
	}
	if(%self.clX >= %self.sizeX)
	{
		%self.loadComplete();
		return;
	}

	%cellType = $Dungeon::Keys[%self.grid[%self.clX, %self.clY]];
	if(%cellType $= "BLANK")
	{
		%self.clY++;
		%self.schedule(%self.loadSpeed, cellLoaded);
		return;
	}
	if(%self.type[%self.clX, %self.clY] > 1)
		%file = $Dungeon::SavesDir @ "dungeon_" @ %self.style @ "_" @ %cellType @ %self.type[%self.clX, %self.clY] @ ".bls";
	else
		%file = $Dungeon::SavesDir @ "dungeon_" @ %self.style @ "_" @ %cellType @ ".bls";
	if(!isFile(%file))
	{
		%altFile = %file = $Dungeon::SavesDir @ "dungeon_" @ %self.style @ "_" @ %cellType @ "1.bls";
		if(!isFile(%altFile))
		{
			error("Dungeon::cellLoaded - !!!!!!LAYOUT DOES NOT HAVE SAVE FOR CELL TYPE " @ %cellType @ "!!!!!!");
			%self.clY++;
			%self.schedule(%self.loadSpeed, cellLoaded);
			return;
		}
		else
			%file = %altFile;
	}
	%cellSize = $Dungeon::LayoutCellSize[%self.style];
	%offset = %self.clX * %cellSize SPC %self.clY * %cellSize SPC 0;
	%rot = (%self.rotation[%self.clX, %self.clY] !$= "" ? %self.rotation[%self.clX, %self.clY] : 0);
	%self.blsParser.setLoadParameters(VectorAdd(%offset, %self.loadOffset), %rot, %self.loadSpeed, %file);
	%self.blsParser.loadStart();
	%self.clY++;
}

function Dungeon::loadComplete(%self)
{
	%self.loadDone = true;
	%self.blsParser.delete();
}

function createDungeon(%name, %floor, %size, %rooms, %roomx, %roomy, %seed, %len, %style, %decorDiv)
{
	if(%name $= "") //documentation because i'm too lazy to remember my own script apparently
	{
		warn("createDungeon(%name, %floor, %size, %rooms, %roomx, %roomy, %seed, %len, %style, %decorDiv) :");
		warn("returns a Dungeon ScriptObject and generates it with the parameters provided");
		return;
	}

	if(%seed $= "")
		%seed = getRealTime();

	if(%len $= "" || %len < 0)
		%len = 0;

	if(%style $= "" || findWord($Dungeon::Types, %style) == -1)
		%style = $Dungeon::DefaultType;

	setRandomSeed(%seed + (%floor * 32));
	%this = newDungeon(%name, getWord(%size, 0), getWord(%size, 1), %rooms, getWord(%roomx, 0), getWord(%roomy, 0), getWord(%roomx, 1), getWord(%roomy, 1), %style, %decorDiv);
	if(!isObject(%this))
		return;
	messageAll('', "<font:verdana bold:32><shadow:1:1><shadowcolor:CCCCCC>\c3" @ %name);
	messageAll('', "<font:verdana bold:24>\c6Floor " @ %floor @ "; Seed " @ %seed);
	%this.len = %len;
	%this.generate();
	return %this;
}

package DungeonBuild
{
	function BLSObject::loadComplete(%self)
	{
		parent::loadComplete(%self);
		if(isObject($Dungeon))
		{
			if($Dungeon.blsParser == %self)
				$Dungeon.schedule($Dungeon.loadSpeed, cellLoaded);
		}
	}
};
activatePackage(DungeonBuild);

schedule(0, 0, Dungeon_loadDefaultStyles); //Causes issues related to file finding when called on the same tick.