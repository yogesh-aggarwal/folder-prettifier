import PySimpleGUI as sg
import os
from pathlib import Path

# from time import sleep
import platform

sg.theme("reddit")
values = None
win = None


class Tools:
    def __init__(self):
        self.location = os.getcwd()
        self.platform = platform.system()
        self.files = None
        self.locationFormat = "/" if self.platform == "Linux" else "\\"

    def getFolderName(self, location=""):
        if self.platform == "Linux":
            location = self.location.split("/") if not location else location.split("/")
            print(location)
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
        win.Element("_status_").Update(status)

    def specificWord(self):
        self.files = self.getItems(self.location)
        for file in self.files:
            os.rename(file, file.replace(values["_removeWord_"], values["_withWord_"]))

    def underscore(self):
        self.files = self.getItems(self.location)
        for file in self.files:
            os.rename(file, file.replace("_", " "))

    def capitalize(self):
        self.files = self.getItems(self.location)
        self.status("capitalizing files")
        for file in self.files:
            os.rename(file, file.capitalize())

    # def casing(self):
    #     print("casing")

    def spacing(self):
        self.files = self.getItems(self.location)
        for file in self.files:
            with open(file, "w+") as file:
                file.write(
                    [
                        file.read()
                        .replace(f"{x} ", x)
                        .replace(f" {x}", x)
                        .replace(x, f"{x} ")
                        for x in [".", ",", ":", "!", "?"]
                    ]
                )

    def categorize(self):
        self.files = self.getItems(self.location)
        extention = {
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
        filesNo = len(self.files)
        for i in range(filesNo):
            for j in range(len(extention["image"])):
                # Scripts
                try:
                    if self.files[i].lower().endswith(extention["script"][j]):
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
                    if self.files[i].lower().endswith(extention["video"][j]):
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
                    if self.files[i].lower().endswith(extention["music"][j]):
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
                    if self.files[i].lower().endswith(extention["image"][j]):
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
                    if self.files[i].lower().endswith(extention["text"][j]):
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
                    if self.files[i].lower().endswith(extention["otherDocs"][j]):
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
                    if self.files[i].lower().endswith(extention["compression"][j]):
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
                    if self.files[i].lower().endswith(extention["app"][j]):
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
            self.status(f"Categorising | Files left = {filesNo}")

    def rename(self):
        os.chdir(Path.home())
        new = self.location.replace(self.getFolderName(), values["_rename_"])
        os.rename(self.location, new)
        os.chdir(new)

    def action(self, values):
        actions = (
            (values["_opSpecificWord_"], self.specificWord),
            (values["_opUnderscore_"], self.underscore),
            (values["_opCapitalize_"], self.capitalize),
            # (values["_opCasing_"], self.casing),
            (values["_opSpacing_"], self.spacing),
            (values["_opCategorize_"], self.categorize),
            (True, self.rename),
        )
        self.status("Starting...")
        self.location = values["_location_"]
        self.status("Getting files ready...")
        os.chdir(self.location)
        [action[1]() for action in actions if action[0]]
        self.status("Done!")


class Window(Tools):
    def __init__(self):
        super().__init__()
        self.win = None
        self.font = "SF UI Text"
        self.layout = [
            [sg.Text("Folder", font=(self.font, 18), pad=(0, 5, 0, 0))],
            [
                sg.Text("Location"),
                sg.InputText(
                    default_text=self.location, change_submits=True, key="_location_", pad=(20, 0, 0, 0)
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
            [sg.Text("Replace", pad=(25, 0, 0, 0), text_color="grey", key="_removeWordText_"), sg.Input(key="_removeWord_", disabled=True),],
            [sg.Text("With", pad=(25, 0, 0, 25), text_color="grey", key="_withWordText_"), sg.Input(key="_withWord_", disabled=True, pad=(26, 0, 0, 0)),],
            # [sg.Checkbox("Handle casing mistakes in text files", key="_opCasing_")],
            [sg.Checkbox("Handle spacing mistakes in text files", key="_opSpacing_")],
            [
                sg.Checkbox(
                    "Categorize files in respective folders", key="_opCategorize_"
                )
            ],
            [sg.Button("Action", pad=(0, 8, 0, 8), key="_action_")],
            [sg.Text(f"Ready{' '*100}", pad=(0, 8, 0, 8), key="_status_")],
        ]

    def createWin(self):
        self.win = sg.Window("Folder Prettfier", self.layout, resizable=0)

    def operate(self):
        global values
        while True:
            event, values = self.win.read()
            if event in (None, "Cancel"):
                break
            if event == "_location_":
                self.win.Element("_rename_").Update(
                    self.getFolderName(values["_location_"])
                )
                files = self.getItems(values["_location_"])
                files = len(files) if files != None else "Folder doesn't exists"
                self.win.Element("_noFiles_").Update(f"Files: {files}")
            if event == "_opSpecificWord_":
                self.win.Element("_removeWord_").Update(disabled=False) if values[
                    "_opSpecificWord_"
                ] else self.win.Element("_removeWord_").Update(disabled=True)
                self.win.Element("_removeWordText_").Update(text_color="black") if values[
                    "_opSpecificWord_"
                ] else self.win.Element("_removeWordText_").Update(text_color="grey")

                self.win.Element("_withWord_").Update(disabled=False) if values[
                    "_opSpecificWord_"
                ] else self.win.Element("_withWord_").Update(disabled=True)
                self.win.Element("_withWordText_").Update(text_color="black") if values[
                    "_opSpecificWord_"
                ] else self.win.Element("_withWordText_").Update(text_color="grey")

            if event == "_action_":
                global win
                win = self.win
                self.action(values)

    def close(self):
        self.win.close()


# Window operations
main = Window()
main.createWin()
main.operate()
main.close()
