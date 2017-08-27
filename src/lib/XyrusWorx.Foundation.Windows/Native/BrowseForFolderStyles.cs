using System;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Native
{
	[Flags, PublicAPI]
	enum BrowseForFolderStyles
	{
		RestrictToFilesystem = 0x0001, // BIF_RETURNONLYFSDIRS
		RestrictToDomain = 0x0002, // BIF_DONTGOBELOWDOMAIN
		RestrictToSubfolders = 0x0008, // BIF_RETURNFSANCESTORS
		ShowTextBox = 0x0010, // BIF_EDITBOX
		ValidateSelection = 0x0020, // BIF_VALIDATE
		NewDialogStyle = 0x0040, // BIF_NEWDIALOGSTYLE
		BrowseForComputer = 0x1000, // BIF_BROWSEFORCOMPUTER
		BrowseForPrinter = 0x2000, // BIF_BROWSEFORPRINTER
		BrowseForEverything = 0x4000, // BIF_BROWSEINCLUDEFILES
	}
}