import os
import platform
from warnings import filterwarnings
import time
from json import loads
from pathlib import Path
from tempfile import gettempdir

import PySimpleGUI as sg
from requests import get

filterwarnings("ignore")

sg.theme("Reddit")
values = None
mainWin = None


class Tools:
    def __init__(self):
        # & Program variables
        self.location = os.getcwd()
        self.platform = platform.system()
        self.files = None
        self.locationFormat = "/" if self.platform == "Linux" else "\\"
        self.progress = False

        # & Program attributes
        self.version = "v1.0.0"
        self.catalog = {
            "video": (
                ".mp4",
                ".mkv",
                ".m4a",
                "m4v",
                ".f4v",
                ".f4a",
                ".m4b",
                ".m4r",
                ".f4b",
                ".mov",
                ".3gp",
                ".3gp2",
                ".3g2",
                ".3gpp",
                ".3gpp2",
                ".ogg",
                ".oga",
                ".ogv",
                ".ogx",
                ".wmv",
                ".wma",
                ".asf",
                ".webm",
                ".flv",
                ".avi",
                ".OP1a",
                ".OP-Atom",
                ".ts",
                ".wav",
                ".lxf",
                ".gxf",
                ".vob",
            ),
            "music": (
                ".mp3",
                ".aa",
                ".aac",
                ".aax",
                ".act",
                ".aiff",
                ".amr",
                ".ape",
                ".au",
                ".awb",
                ".dct",
                ".dss",
                ".dvf",
                ".flac",
                ".gsm",
                ".iklax",
                ".ivs",
                ".m4a",
                ".m4b",
                ".m4p",
                ".mmf",
                ".mp3",
                ".mpc",
                ".msv",
                ".nmf",
                ".nsf",
                ".ogg",
                ".oga",
                ".mogg",
                ".opus",
                ".ra",
                ".rm",
                ".raw",
                ".sln",
                ".tta",
                ".vox",
                ".wav",
                ".wma",
                ".wv",
                ".8svx",
            ),
            "image": (
                ".jfif",
                ".bmp",
                ".gif",
                ".jpeg",
                ".jpg",
                ".png",
                ".bpg",
                ".deep",
                ".drw",
                ".ecw",
                ".fits",
                ".flif",
                ".ico",
                ".ilbm",
                ".img",
                ".nrrd",
                ".pam",
                ".pcx",
                ".pgf",
                ".sgi",
                ".sid",
                ".tga",
                ".cd5",
                ".cpt",
                ".psd",
                ".xcf",
                ".pdn",
                ".cgm",
                ".afdesign",
                ".al",
                ".cdr",
                ".gem",
                ".hpgl",
                ".hvif",
                ".athml",
                ".naplps",
                ".odg",
                ".qcc",
                ".regis",
                ".vml",
                ".xar",
                ".xps",
                ".amf",
                ".blend",
                ".dgn",
                ".dwg",
                ".dxf",
                ".flt",
                ".fvrml",
                ".hsf",
                ".iges",
                ".imml",
                ".ipa",
                ".jt",
                ".ma",
                ".mb",
                ".ob",
                ".ply",
                ".prc",
                ".step",
                ".skp",
                ".stl",
                ".u3d",
                ".vrml",
                ".xaml",
                ".xgl",
                ".xvl",
                "xvrml",
                ".x3d",
                ".3d",
                ".3df",
                ".3ds",
                ".3dxml",
                ".jps",
                ".svg",
            ),
            "text": (".txt", ".rtf", ".info", ".md", ".prn", ".md"),
            "compression": (".zip", ".iso", ".rar", ".7z", ".gz", ".dmg"),
            "app": (".exe", ".msi"),
            "script": (
                ".py",
                ".pyc",
                ".js",
                ".json",
                ".html",
                ".xml",
                ".css",
                ".c",
                ".cs",
                ".r",
                ".java",
                ".cmd",
                ".bat",
                ".db",
                ".sql",
                ".dll",
            ),
            "otherDocs": (
                ".pdf",
                ".docx",
                ".docm",
                ".doc",
                ".dotm",
                ".dotx",
                ".dot",
                ".csv",
                ".xlsm",
                ".xlm",
                ".xls",
                ".xps",
                ".pptx",
                ".pptm",
                ".ppt",
                ".pot",
                ".potx",
                ".potm",
                ".pps",
                ".ppsm",
                ".ppa",
                "ppam",
                ".eml",
            ),
        }

    def getFolderName(self, location=""):
        if self.platform == "Linux":
            location = self.location.split("/") if not location else location.split("/")
            return (
                location[len(location) - 1]
                if location[len(location) - 1]
                else location[len(location) - 2]
            )

    def getItems(self, location=""):
        try:
            location = location if location else self.location
            return os.listdir(location)
        except Exception:
            return None

    def status(self, status):
        try:
            mainWin.Element("_status_").Update(status)
            mainWin.Refresh()
        except Exception:
            pass

    def specificWord(self):
        self.files = self.getItems(self.location)
        filesNo = len(self.files)
        for file in self.files:
            os.rename(file, file.replace(values["_removeWord_"], values["_withWord_"]))
            filesNo -= 1
            self.status(f"Renaming files | Files left = {filesNo}")

    def underscore(self):
        self.files = self.getItems(self.location)
        filesNo = len(self.files)
        for file in self.files:
            os.rename(file, file.replace("_", " "))
            filesNo -= 1
            self.status(f"Remove underscores | Files left = {filesNo}")

    def capitalize(self):
        self.files = self.getItems(self.location)
        self.status("Capitalizing files")
        filesNo = len(self.files)
        for file in self.files:
            filesNo -= 1
            os.rename(file, file.capitalize())
            self.status(f"Capitalizing names | Files left = {filesNo}")

    def spacing(self):
        self.files = self.getItems(self.location)
        filesNo = len(self.files)
        for file in self.files:
            filesNo -= 1
            with open(file, "w+") as file:
                file.writelines(
                    [
                        file.read()
                        .replace(f"{x} ", x)
                        .replace(f" {x}", x)
                        .replace(x, f"{x} ")
                        for x in [".", ",", ":", "!", "?"]
                    ]
                )
            self.status(f"Spacing files | Files left = {filesNo}")

    def arrangeFiles(self):
        self.files = sorted(self.getItems(self.location))
        filesNo = len(self.files)
        for index, file in enumerate(self.files):
            filesNo -= 1
            os.rename(
                file, f"{time.time()}.{file.split('.')[len(file.split('.'))-1]}",
            )
        self.files = sorted(self.getItems(self.location))
        for index, file in enumerate(self.files):
            filesNo -= 1
            os.rename(
                file,
                f"{values['_arrangeKeyword_']}{str(index+1)}.{file.split('.')[len(file.split('.'))-1]}",
            )
            self.status(f"Arranging files | Files left = {filesNo}")

    def categorize(self):
        self.files = self.getItems(self.location)

        filesNo = len(self.files)
        for i in range(filesNo):
            for j in range(len(self.catalog["image"])):
                # Scripts
                try:
                    if self.files[i].lower().endswith(self.catalog["script"][j]):
                        try:
                            os.mkdir("Documents")
                        except:
                            pass
                        # Folder Creation
                        try:
                            os.mkdir(f"Documents{self.locationFormat}Scripts")
                        except:
                            pass
                        try:
                            os.rename(
                                self.files[i],
                                f"Documents{self.locationFormat}Scripts{self.locationFormat}{self.locationFormat}{self.files[i]}",
                            )
                        except:
                            pass
                        os.chdir(self.location)
                except:
                    pass

                # Video
                try:
                    if self.files[i].lower().endswith(self.catalog["video"][j]):
                        # Folder Creation
                        try:
                            os.mkdir("Videos")
                        except:
                            pass
                        try:
                            os.rename(
                                self.files[i],
                                f"Videos{self.locationFormat}{self.locationFormat}{self.files[i]}",
                            )
                        except:
                            pass
                except:
                    pass

                # Music
                try:
                    if self.files[i].lower().endswith(self.catalog["music"][j]):
                        # Folder Creation
                        try:
                            os.mkdir("Music")
                        except:
                            pass
                        try:
                            os.rename(
                                self.files[i],
                                f"Music{self.locationFormat}{self.locationFormat}{self.files[i]}",
                            )
                        except:
                            pass
                except:
                    pass

                # Image
                try:
                    if self.files[i].lower().endswith(self.catalog["image"][j]):
                        # Folder Creation
                        try:
                            os.mkdir("Images")
                        except:
                            pass
                        try:
                            os.rename(
                                self.files[i],
                                f"Images{self.locationFormat}{self.locationFormat}{self.files[i]}",
                            )
                        except:
                            pass
                except:
                    pass

                # Text
                try:
                    if self.files[i].lower().endswith(self.catalog["text"][j]):
                        try:
                            os.mkdir("Documents")
                        except:
                            pass
                        # Folder Creation
                        try:
                            os.mkdir(f"Documents{self.locationFormat}text files")
                        except:
                            pass
                        try:
                            os.rename(
                                self.files[i],
                                f"Documents{self.locationFormat}text files{self.locationFormat}{self.locationFormat}{self.files[i]}",
                            )
                        except:
                            pass
                        os.chdir(self.location)
                except:
                    pass

                # Other docs
                try:
                    if self.files[i].lower().endswith(self.catalog["otherDocs"][j]):
                        try:
                            os.mkdir("Documents")
                        except:
                            pass
                        # Folder Creation
                        try:
                            os.mkdir(f"Documents{self.locationFormat}Other document")
                        except:
                            pass
                        try:
                            os.rename(
                                self.files[i],
                                f"Documents{self.locationFormat}{self.locationFormat}Other document{self.locationFormat}{self.locationFormat}{self.files[i]}",
                            )
                        except:
                            pass
                        os.chdir(self.location)
                except:
                    pass

                # Compression
                try:
                    if self.files[i].lower().endswith(self.catalog["compression"][j]):
                        # Folder Creation
                        try:
                            os.mkdir("Compressions")
                        except:
                            pass
                        try:
                            os.rename(
                                self.files[i],
                                f"Compressions{self.locationFormat}{self.locationFormat}{self.files[i]}",
                            )
                        except:
                            pass
                except:
                    pass

                # Apps
                try:
                    if self.files[i].lower().endswith(self.catalog["app"][j]):
                        # Folder Creation
                        try:
                            os.mkdir("Apps")
                        except:
                            pass
                        try:
                            os.rename(
                                self.files[i],
                                f"Apps{self.locationFormat}{self.locationFormat}{self.files[i]}",
                            )
                        except:
                            pass
                except:
                    pass
            filesNo -= 1
            self.status(f"Categorizing | Files left = {filesNo}")

    def rename(self):
        self.status(
            f"Renaming folder '{self.getFolderName()}' -> '{values['_rename_']}'"
        )
        os.chdir(Path.home())
        new = self.location.replace(self.getFolderName(), values["_rename_"])
        os.rename(self.location, new)
        os.chdir(new)

    def action(self, values):
        actions = (
            (values["_opSpecificWord_"], self.specificWord),
            (values["_opUnderscore_"], self.underscore),
            (values["_opCapitalize_"], self.capitalize),
            (values["_opArrange_"], self.arrangeFiles),
            # (values["_opCasing_"], self.casing),
            (values["_opSpacing_"], self.spacing),
            (values["_opCategorize_"], self.categorize),
            (True, self.rename),
        )
        self.progress = True
        self.status("Starting...")
        self.location = values["_location_"]
        self.status("Getting files ready...")
        os.chdir(self.location)
        [action[1]() for action in actions if action[0]]
        self.status("Done!")
        sg.Popup(
            "Your folder is prettified and looks nice now!",
            title="Done",
            keep_on_top=True,
        )
        self.status("Ready!")


class OtherOptions(Tools):
    def __init__(self):
        super().__init__()

    def updateCatalog(self, user=False):
        if not self.progress:
            try:
                self.status("Updating catalog...")
                data = get(
                    f"https://raw.githubusercontent.com/yogesh-aggarwal/folder-prettifier/master/docs/{self.version}.json"
                ).text
                with open(f"{gettempdir()}/catalog[folder-prettifier]", "w+") as f:
                    f.write(data)
                    self.catalog = loads(data)["extensions"]
                self.status("Ready!")
                sg.Popup(
                    "Catalog updated successfully!", title="Success"
                ) if user else False
            except Exception as e:
                raise e
                self.status("No internet using previous cache...")
                sg.Popup(
                    "Unable to retrieve the catalog from the server due to connectiviy issues!",
                    title="No internet connection",
                ) if user else False
                with open(f"{gettempdir()}/catalog[folder-prettifier]", "r+") as f:
                    try:
                        self.catalog = loads(f.read())["extensions"]
                    except Exception:
                        sg.Popup(
                            "Unable to parse the cache, using built-in catalog",
                            title="Data incorrect",
                        ) if user else False
            self.status("Ready!")
        else:
            sg.PopupAutoClose(
                "You can't make changes while the progress is going on!", title="Notice"
            )

    def update(self):
        sg.Popup(
            "Checking for updates...",
            "Once done, program will automatically start",
            title="Update",
            auto_close=True,
            auto_close_duration=1,
        )
        data = get(
            f"https://raw.githubusercontent.com/yogesh-aggarwal/folder-prettifier/master/docs/attributes.json"
        ).text
        if loads(data)["latestVersion"] > "v0.1.1":
            sg.Popup(
                f'Update to {loads(data)["version"]} (from {self.version}) found!',
                "Now the program will be updating. It might take a while depending upon your internet connection",
                title="Update",
                keep_on_top=True,
            )
            

            return True
        else:
            print("Already the latest version!!!")

    def about(self):
        sg.Popup(
            "Folder prettfier is a program which will help you to organize your folders & files with ease.",
            f"Version: {self.version}",
            title="About",
            keep_on_top=True,
        )


class Window(OtherOptions):
    def __init__(self):
        super().__init__()
        self.mainWin = None
        self.font = "SF UI Text"
        self.layout = [
            [sg.Text("Folder", font=(self.font, 18), pad=(0, 5, 0, 0))],
            [
                sg.Text("Location"),
                sg.InputText(
                    default_text=self.location,
                    change_submits=True,
                    key="_location_",
                    pad=(20, 0, 0, 0),
                ),
            ],
            [
                sg.Text("Rename to"),
                sg.InputText(
                    default_text=self.getFolderName(),
                    change_submits=True,
                    key="_rename_",
                ),
            ],
            [
                sg.Text(
                    f'Files: {len(os.listdir(os.getcwd()))}{" "*40}', key="_noFiles_"
                )
            ],
            [sg.Text("Tools", font=(self.font, 18), pad=(0, 10, 0, 5))],
            [sg.Checkbox("Capitalize names", key="_opCapitalize_")],
            [sg.Checkbox("Remove underscore in names", key="_opUnderscore_")],
            [
                sg.Checkbox(
                    "Remove specific word in names",
                    key="_opSpecificWord_",
                    change_submits=True,
                ),
            ],
            [
                sg.Text(
                    "Replace",
                    pad=(25, 0, 0, 0),
                    text_color="grey",
                    key="_removeWordText_",
                ),
                sg.Input(key="_removeWord_", disabled=True),
            ],
            [
                sg.Text(
                    "With", pad=(25, 0, 0, 25), text_color="grey", key="_withWordText_"
                ),
                sg.Input(key="_withWord_", disabled=True, pad=(26, 0, 0, 0)),
            ],
            # [sg.Checkbox("Handle casing mistakes in text files", key="_opCasing_")],
            [sg.Checkbox("Handle spacing mistakes in text files", key="_opSpacing_")],
            [sg.Checkbox("Arrange files", change_submits=True, key="_opArrange_"),],
            [
                sg.Text(
                    "Name starts with", text_color="grey", key="_arrangeKeywordText_"
                ),
                sg.InputText(key="_arrangeKeyword_", disabled=True),
            ],
            [
                sg.Checkbox(
                    "Categorize files in respective folders", key="_opCategorize_"
                )
            ],
            [
                sg.Button("Action", pad=(0, 8, 0, 8), key="_action_"),
                sg.Button(
                    "Update catalog",
                    tooltip="Update the catalog for better file catagorizing in different folders",
                    key="_updateCatalog_",
                ),
                sg.Button("About", key="_about_",),
            ],
            [sg.Text(f"Ready!{' '*100}", pad=(0, 8, 0, 8), key="_status_")],
        ]

    def createWin(self):
        global mainWin
        self.mainWin = sg.Window(
            "Folder Prettfier", self.layout, resizable=0, icon="icon.ico"
        )
        mainWin = self.mainWin

    def operate(self):
        global values
        while True:
            event, values = self.mainWin.read()
            if event in (None, "Cancel"):
                break

            # & For options
            if event == "_location_":
                self.mainWin.Element("_rename_").Update(
                    self.getFolderName(values["_location_"])
                )
                files = self.getItems(values["_location_"])
                files = len(files) if files != None else "Folder doesn't exists"
                self.mainWin.Element("_noFiles_").Update(f"Files: {files}")
            elif event == "_opSpecificWord_":
                self.mainWin.Element("_removeWord_").Update(disabled=False) if values[
                    "_opSpecificWord_"
                ] else self.mainWin.Element("_removeWord_").Update(disabled=True)
                self.mainWin.Element("_removeWordText_").Update(
                    text_color="black"
                ) if values["_opSpecificWord_"] else self.mainWin.Element(
                    "_removeWordText_"
                ).Update(
                    text_color="grey"
                )

                self.mainWin.Element("_withWord_").Update(disabled=False) if values[
                    "_opSpecificWord_"
                ] else self.mainWin.Element("_withWord_").Update(disabled=True)
                self.mainWin.Element("_withWordText_").Update(
                    text_color="black"
                ) if values["_opSpecificWord_"] else self.mainWin.Element(
                    "_withWordText_"
                ).Update(
                    text_color="grey"
                )

            # & For other actions
            if event == "_updateCatalog_":
                self.updateCatalog(user=True)
            elif event == "_about_":
                self.about()
            elif event == "_action_":
                self.action(values)
            elif event == "_settings_":
                print("d")
            elif event == "_opArrange_":
                self.mainWin.Element("_arrangeKeyword_").Update(
                    disabled=False
                ) if values["_opArrange_"] else self.mainWin.Element(
                    "_arrangeKeyword_"
                ).Update(
                    disabled=True
                )
                self.mainWin.Element("_arrangeKeywordText_").Update(
                    text_color="black"
                ) if values["_opArrange_"] else self.mainWin.Element(
                    "_arrangeKeywordText_"
                ).Update(
                    text_color="grey"
                )

    def close(self):
        self.mainWin.close()


# & Window operations
main = Window()
if not main.update():
    main.updateCatalog()
    main.createWin()
    main.operate()
    main.close()
