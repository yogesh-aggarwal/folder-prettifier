from tkinter import (
    Tk,
    StringVar,
    IntVar,
    Frame,
    Label,
    Listbox,
    Scrollbar,
    X,
    W,
    BOTTOM,
    SUNKEN,
    MULTIPLE,
)
from tkinter import ttk
from tkinter.filedialog import askdirectory
from tkinter.messagebox import showerror, showinfo
import os

locationFormat = "/"

main = Tk()
main.title("Folder Prettifier")
main.geometry("800x515")

selection = False

filesLeft = []


def getFolder(*awargs):
    global selection
    try:
        if awargs[0] == "current":
            folder = os.getcwd()
    except:
        folder = askdirectory()
    folderLocation.set(folder)

    try:
        os.chdir(folderLocation.get())
        filesNo = len(os.listdir())
        for i in range(filesNo):
            l.insert(i, os.listdir()[i])

        statusBar.set(f"No.of files = {filesNo-1}")
        selection = True
    except:
        showerror("Error! ", "Location not found! Please choose a valid location...")


def capitalizeFiles(*awargs):
    filesNo = len(os.listdir())

    files = os.listdir()
    for i in range(len(files)):
        try:
            os.rename(
                files[i],
                files[i]
                .capitalize()
                .replace("_", " ")
                .replace("  ", " ")
                .rstrip()
                .lstrip(),
            )
            filesNo -= 1
            statusBar.set(
                f"Capitalizing and removing underscores | Files left = {filesNo}"
            )
            status.update()
            main.update()
        except:
            pass


def handleCasingMistakes(*awargs):
    filesNo = len(os.listdir())

    files = os.listdir()
    for i in range(len(files)):
        filesNo -= 1
        statusBar.set(f"Handling casing mistakes | Files left = {filesNo}")
        text = (".txt", ".rtf", ".info", ".md", ".prn", ".md")
        ext = "." + files[i].split(".")[-1]
        if ext in text:
            try:
                # print(f"File ---> {files[i]}")
                puncLst = [". ", "! ", "? "]
                for punc in puncLst:
                    with open(files[i], "r") as f:
                        wordList = f.read().split(punc)
                        for i in range(len(wordList)):
                            print(wordList)
                            wordList[i] = wordList[i].capitalize()
                        print(wordList)
                    f = open(files[i], "w")
                    f.write(". ".join(wordList))
                    f.close()
            except Exception as e:
                print(e)

        status.update()
        main.update()


def handleSpacingMistakes(*awargs):
    filesNo = len(os.listdir())

    files = os.listdir()
    text = (".txt", ".rtf", ".info", ".md", ".prn", ".md")
    for i in range(len(files)):
        ext = "." + files[i].split(".")[-1]
        if ext in text:
            try:
                file = open(files[i], "r")
                readData = file.read()
                file.close()
                file = open(files[i], "w")
                file.write(
                    readData.replace(", ", ", ")
                    .replace(", ", ", ")
                    .replace(": ", ": ")
                    .replace(": ", ": ")
                    .replace(".", ".")
                    .replace(".", ".")
                    .replace("! ", "! ")
                    .replace("! ", "! ")
                    .replace("? ", "? ")
                    .replace("? ", "? ")
                )
                file.close()
            except:
                pass
            filesNo -= 1
            statusBar.set(f"Handling spacing mistakes | Files left = {filesNo}")
            status.update()
            main.update()


def categoriseFilesInFolders(*awargs):
    location = folderLocation.get()
    filesNo = len(os.listdir())

    files = os.listdir()

    # Extensions
    

    files = os.listdir()


def applyToSubFolders(*awargs):
    pass


def start(*awargs):
    try:
        os.chdir(folderLocation.get())
        selection = True
    except:
        showerror("Error! ", "Location not found! Please choose a valid location...")
    statusBar.set("Working on it...")
    status.update()

    # Extras
    print(capitalizeFilesVar.get())
    print(handleCasingMistakesVar.get())
    print(handleSpacingMistakesVar.get())
    print(applyToSubFoldersVar.get())

    print(filesLeft)

    if capitalizeFilesVar.get() == 1:
        capitalizeFiles()
    if handleCasingMistakesVar.get() == 1:
        handleCasingMistakes()
    if handleSpacingMistakesVar.get() == 1:
        handleSpacingMistakes()
    if applyToSubFoldersVar.get() == 1:
        applyToSubFolders()
    if categoriseFilesInFoldersVar.get() == 1:
        categoriseFilesInFolders()

    statusBar.set("Ready! ")
    status.update()

    try:
        if selection:
            print(f"\nDone!\n")
            showinfo(
                "Done! ", "Your folder has successfully Prettified and looks great now."
            )
            os.startfile(folderLocation.get())
    except:
        pass


# Variables
folderLocation = StringVar()
statusBar = StringVar()
filesNoLabelLabelVar = IntVar()

capitalizeFilesVar = IntVar()
handleCasingMistakesVar = IntVar()
handleSpacingMistakesVar = IntVar()
categoriseFilesInFoldersVar = IntVar()
applyToSubFoldersVar = IntVar()

style = ttk.Style()

frameLabel = Frame(main)
frameLabel.pack()
Label(
    frameLabel,
    text="Enter/Choose the folder location",
    font=("google sans bold", 15),
    fg="red",
).pack()

frame1 = Frame(main)
frame1.pack()

ttk.Entry(frame1, textvariable=folderLocation).grid(row=1, column=0)
Label(frame1, text=" ").grid(row=1, column=1)
ttk.Button(frame1, text="...", command=getFolder).grid(row=1, column=2)
Label(frame1, text=" ").grid(row=1, column=3)
ttk.Button(frame1, text="Current location", command=lambda: getFolder("current")).grid(
    row=1, column=4
)
Label(frame1, text=" ").grid(row=1, column=5)

Label(main, text=" ").pack()

frame2 = Frame(main)
frame2.pack()


def removeFromList():
    clicked = l.curselection()
    for item in clicked:
        filesLeft.append(item)
    for poss in filesLeft:
        l.delete(poss)


l = Listbox(frame2, width=120, height=10, selectmode=MULTIPLE)
l.pack(side="left")
scrollbar = Scrollbar(frame2, orient="vertical")
scrollbar.config(command=l.yview)
scrollbar.pack(side="right", fill="y")

l.config(yscrollcommand=scrollbar.set)

frame3 = Frame(main)
frame3.pack(fill=X)


Label(frame3, text=" ").grid(row=1)
Label(frame3, text=f"\t\t\t\t\t\t\t    ").grid(row=2, column=1)
ttk.Button(frame3, text="Exclude Files", command=removeFromList).grid(row=2, column=2)

Label(main, text=" ").pack()

frame4 = Frame(main)
frame4.pack(fill=X)

style.configure("TCheckbutton", font=("google sans regular", 11))
ttk.Checkbutton(
    frame4,
    style="TCheckbutton",
    text="Capitalize and remove underscores from name of files",
    variable=capitalizeFilesVar,
    onvalue=1,
    offvalue=0,
).pack(anchor=W)
ttk.Checkbutton(
    frame4,
    style="TCheckbutton",
    text="Handle casing mistakes in all text files",
    variable=handleCasingMistakesVar,
    onvalue=1,
    offvalue=0,
).pack(anchor=W)
ttk.Checkbutton(
    frame4,
    style="TCheckbutton",
    text="Handle all the spacing mistakes after fullstops, commas, colons etc...",
    variable=handleSpacingMistakesVar,
    onvalue=1,
    offvalue=0,
).pack(anchor=W)
ttk.Checkbutton(
    frame4,
    style="TCheckbutton",
    text="Categories files in different folders.",
    variable=categoriseFilesInFoldersVar,
    onvalue=1,
    offvalue=0,
).pack(anchor=W)
# ttk.Checkbutton(frame4, style="TCheckbutton", text="Apply changes to all the sub-folders", variable=applyToSubFoldersVar, onvalue=1, offvalue=0).pack(anchor=W)

Label(frame4, text="\n").pack()

ttk.Button(frame4, text="Prettify", command=start).pack()

Label(frame4, text="\n").pack()

status = Label(frame4, textvariable=statusBar, bd=1, relief=SUNKEN, anchor=W)
status.pack(fill=X, side=BOTTOM)

statusBar.set("Ready! ")

main.grab_set()

main.mainloop()
