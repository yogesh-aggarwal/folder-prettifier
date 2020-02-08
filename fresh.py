import PySimpleGUI as sg
import os
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
    
    def getFolderName(self, location=""):
        if self.platform == "Linux":
            location = self.location.split("/") if not location else location.split("/")
            print(location)
            return location[len(location)-1] if location[len(location)-1] else location[len(location)-2]
    
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
            os.rename(file, file.replace(values["_removeWord_"], ""))
    
    def underscore(self):
        self.files = self.getItems(self.location)
        for file in self.files:
            os.rename(file, file.replace("_", " "))

    def capitalize(self):
        self.files = self.getItems(self.location)
        self.status("capitalizing files")
        for file in self.files:
            os.rename(file, file.capitalize())
    
    def casing(self):
        print("casing")
    
    def spacing(self):
        self.files = self.getItems(self.location)
        for file in self.files:
            with open(file, "w+") as file:
                file.write([file.read().replace(f"{x} ", x).replace(f" {x}", x).replace(x, f"{x} ") for x in [".", ",", ":", "!", "?"]])
    
    def categorize(self):
        print("categorize")

    def action(self, values):
        actions = (
            (values["_opSpecificWord_"], self.specificWord),
            (values["_opUnderscore_"], self.underscore),
            (values["_opCapitalize_"], self.capitalize),
            # (values["_opCasing_"], self.casing),
            (values["_opSpacing_"], self.spacing),
            (values["_opCategorize_"], self.categorize)
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
            [sg.Text("Location"), sg.InputText(default_text=self.location, change_submits=True, key="_location_")],
            [sg.Text("Rename to"), sg.InputText(default_text=self.getFolderName(), change_submits=True, key="_rename_")],
            [sg.Text(f'Files: {len(os.listdir(os.getcwd()))}{" "*40}', key="_noFiles_")],
            
            [sg.Text("Tools", font=(self.font, 18), pad=(0, 10, 0, 5))],
            [sg.Checkbox("Capitalize names", key="_opCapitalize_")],
            [sg.Checkbox("Remove underscore in names", key="_opUnderscore_")],
            [sg.Checkbox("Remove specific word in names", key="_opSpecificWord_", change_submits=True), sg.Input(key="_removeWord_", disabled=True)],
            # [sg.Checkbox("Handle casing mistakes in text files", key="_opCasing_")],
            [sg.Checkbox("Handle spacing mistakes in text files", key="_opSpacing_")],
            [sg.Checkbox("Categorize files in respective folders", key="_opCategorize_")],

            [sg.Button("Action", pad=(0, 8, 0, 8), key="_action_")],

            [sg.Text(f"Ready{' '*100}", pad=(0, 8, 0, 8), key="_status_")]
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
                self.win.Element("_rename_").Update(self.getFolderName(values["_location_"]))
                files = self.getItems(values["_location_"])
                files = len(files) if files != None else "Folder doesn't exists"
                self.win.Element("_noFiles_").Update(f'Files: {files}')
            if event == "_opSpecificWord_":
                self.win.Element("_removeWord_").Update(disabled=False) if values["_opSpecificWord_"] else self.win.Element("_removeWord_").Update(disabled=True)
            
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
