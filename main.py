from tkinter import *
from tkinter import ttk
from tkinter.filedialog import *
from tkinter.messagebox import *

main = Tk()
# main.config(bg="white")
main.title("Folder Prettifier")
# main.geometry("500x290")
main.geometry("800x515")
main.resizable(0, 0)

global selection
selection = False

files_left = []

def get_folder(*awargs): 
    try: 
        if awargs[0] == "current": 
            folder = os.getcwd()
    except: 
        folder = askdirectory()
    folder_location.set(folder)

    try: 
        os.chdir(folder_location.get())
        files_no = len(os.listdir())
        files = []
        for i in range(files_no):
            l.insert(i, os.listdir()[i])

        status_bar.set(f"No.of files = {files_no-1}")
        selection = True
    except: 
        showerror("Error! ", "Location not found! Please choose a valid location...")

def capitalize_files(*awargs): 
    files_no = len(os.listdir())
    file = open(".ignore.info", "a+")
    file.close()

    file = open(".ignore.info", "r")
    files_ignore = file.readlines()
    if ".ignore.info" not in files_ignore or "Prettify.py" not in files_ignore: 
            files_ignore.extend([".ignore.info", os.path.basename(__file__)])
            file.close()

    for i in range(len(files_ignore)): 
            files_ignore[i] = files_ignore[i].rstrip()
    files = os.listdir()
    for i in range(len(files)): 
            if files[i] not in files_ignore: 
                    try: 
                            os.rename(files[i], files[i].capitalize().replace("_", " ").replace("  ", " ").rstrip().lstrip())
                            files_no -= 1
                            status_bar.set(f"Capitalizing and removing underscores | Files left = {files_no}")
                            status.update()
                            main.update()
                    except: 
                            pass

def handle_casing_mistakes(*awargs): 
    files_no = len(os.listdir())
    file = open(".ignore.info", "a+")
    file.close()
    file = open(".ignore.info", "r")
    files_ignore = file.readlines()
    if ".ignore.info" not in files_ignore or __file__ not in files_ignore: 
            files_ignore.extend([".ignore.info", os.path.basename(__file__)])
            file.close()

    for i in range(len(files_ignore)): 
            files_ignore[i] = files_ignore[i].rstrip()
    files = os.listdir()
    for i in range(len(files)): 
        if files[i] not in files_ignore: 
            files_no -= 1
            status_bar.set(f"Handling casing mistakes | Files left = {files_no}")
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

def handle_spacing_mistakes(*awargs): 
    files_no = len(os.listdir())
    file = open(".ignore.info", "a+")
    file.close()
    file = open(".ignore.info", "r")
    files_ignore = file.readlines()
    if ".ignore.info" not in files_ignore or __file__ not in files_ignore: 
            files_ignore.extend([".ignore.info", os.path.basename(__file__)])
            file.close()

    for i in range(len(files_ignore)): 
        files_ignore[i] = files_ignore[i].rstrip()
    files = os.listdir()
    text = (".txt", ".rtf", ".info", ".md", ".prn", ".md")
    for i in range(len(files)): 
        if files[i] not in files_ignore:
            ext = "." + files[i].split(".")[-1]
            if ext in text:
                try: 
                    file = open(files[i], "r")
                    read_data = file.read()
                    file.close()
                    file = open(files[i], "w")
                    file.write(read_data.replace(", ", ", ").replace(", ", ", ").replace(": ", ": ").replace(": ", ": ").replace(".", ".").replace(".", ".").replace("! ", "! ").replace("! ", "! ").replace("? ", "? ").replace("? ", "? "))
                    file.close()
                except: 
                    pass
                files_no -= 1
                status_bar.set(f"Handling spacing mistakes | Files left = {files_no}")
                status.update()
                main.update()
                
def categorise_files_in_folders(*awargs): 
    location = folder_location.get()
    files_no = len(os.listdir())
    
    files = os.listdir()

    # Extensions
    video = (".mp4", ".mkv", ".m4a", "m4v", ".f4v", ".f4a", ".m4b", ".m4r", ".f4b", ".mov", ".3gp", ".3gp2", ".3g2", ".3gpp", ".3gpp2", ".ogg", ".oga", ".ogv", ".ogx", ".wmv", ".wma", ".asf", ".webm", ".flv", ".avi", ".OP1a", ".OP-Atom", ".ts", ".wav", ".lxf", ".gxf", ".vob")
    music = (".mp3", ".aa", ".aac", ".aax", ".act", ".aiff", ".amr", ".ape", ".au", ".awb", ".dct", ".dss", ".dvf", ".flac", ".gsm", ".iklax", ".ivs", ".m4a", ".m4b", ".m4p", ".mmf", ".mp3", ".mpc", ".msv", ".nmf", ".nsf", ".ogg", ".oga", ".mogg", ".opus", ".ra", ".rm", ".raw", ".sln", ".tta", ".vox", ".wav", ".wma", ".wv", ".8svx")
    image = (".jfif", ".bmp" , ".gif", ".jpeg", ".jpg", ".png", ".bpg", ".deep", ".drw", ".ecw", ".fits", ".flif", ".ico", ".ilbm", ".img", ".nrrd", ".pam", ".pcx", ".pgf", ".sgi", ".sid", ".tga", ".cd5", ".cpt", ".psd", ".xcf", ".pdn", ".cgm", ".afdesign", ".al", ".cdr", ".gem", ".hpgl", ".hvif", ".athml", ".naplps", ".odg", ".qcc", ".regis", ".vml", ".xar", ".xps", ".amf", ".blend", ".dgn", ".dwg", ".dxf", ".flt", ".fvrml", ".hsf", ".iges", ".imml", ".ipa", ".jt", ".ma", ".mb", ".ob", ".ply", ".prc", ".step", ".skp", ".stl", ".u3d", ".vrml", ".xaml", ".xgl", ".xvl", "xvrml", ".x3d", ".3d" , ".3df", ".3ds", ".3dxml", ".jps")
    text = (".txt", ".rtf", ".info", ".md", ".prn", ".md")
    compression = (".zip", ".iso", ".rar", ".7z", ".gz")
    app = (".exe", ".msi")
    script = (".py", ".pyc", ".js", ".json", ".html", ".xml", ".css", ".c", ".cs", ".r", ".java", ".cmd", ".bat", ".db", ".sql", ".dll")
    other_docs = (".pdf", ".docx", ".docm", ".doc", ".dotm", ".dotx", ".dot", ".csv", ".xlsm", ".xlm", ".xls", ".xps", ".pptx", ".pptm", ".ppt", ".pot", ".potx", ".potm", ".pps", ".ppsm", ".ppa", "ppam", ".eml")

    file = open(".ignore.info", "a+")
    file.close()
    file = open(".ignore.info", "r")
    files_ignore = file.readlines()
    if ".ignore.info" not in files_ignore or __file__ not in files_ignore: 
            files_ignore.extend([".ignore.info", os.path.basename(__file__)])
            file.close()

    for i in range(len(files_ignore)): 
            files_ignore[i] = files_ignore[i].rstrip()
    files = os.listdir()            

    for i in range(len(files)): 
        if files[i] not in files_ignore: 
            for j in range(len(image)): 
                # Scripts
                try:
                    if files[i].lower().endswith(script[j]): 
                        try: 
                            os.mkdir("Documents")
                        except: 
                            pass
                        print("Script Found")
                        # Folder Creation
                        try: 
                            os.mkdir("Documents\Scripts")
                        except: 
                            pass
                        try: 
                            os.rename(files[i], f"Documents\Scripts\\{files[i]}")
                        except: 
                            pass
                        os.chdir(location)
                except:
                    pass
                    
                # Video
                try: 
                    if files[i].lower().endswith(video[j]): 
                        print("Video Found")
                        # Folder Creation
                        try: 
                            os.mkdir("Videos")
                        except: 
                            pass
                        try: 
                            os.rename(files[i], f"Videos\\{files[i]}")
                        except: 
                            pass
                except: 
                    pass

                # Music
                try: 
                    if files[i].lower().endswith(music[j]): 
                        print("Music Found")
                        # Folder Creation
                        try: 
                            os.mkdir("Music")
                        except: 
                            pass
                        try: 
                            os.rename(files[i], f"Music\\{files[i]}")
                        except: 
                            pass
                except: 
                    pass

                # Image
                try: 
                    if files[i].lower().endswith(image[j]): 
                        print("Image Found")
                        # Folder Creation
                        try: 
                            os.mkdir("Images")
                        except: 
                            pass
                        try: 
                            os.rename(files[i], f"Images\\{files[i]}")
                        except: 
                            pass
                except: 
                    pass

                # Text
                try: 
                    if files[i].lower().endswith(text[j]): 
                        try: 
                            os.mkdir("Documents")
                        except: 
                            pass
                        print("Text Found")
                        # Folder Creation
                        try: 
                            os.mkdir("Documents\Text files")
                        except: 
                            pass
                        try: 
                            os.rename(files[i], f"Documents\\Text files\\{files[i]}")
                        except: 
                            pass
                        os.chdir(location)
                except: 
                    pass

                # Other docs
                try: 
                    if files[i].lower().endswith(other_docs[j]): 
                        try: 
                            os.mkdir("Documents")
                        except: 
                            pass
                        print("Other doc found Found")
                        # Folder Creation
                        try: 
                            os.mkdir("Documents\Other document")
                        except: 
                            pass
                        try: 
                            os.rename(files[i], f"Documents\\Other document\\{files[i]}")
                        except: 
                            pass
                        os.chdir(location)
                except: 
                    pass

                # Compression
                try: 
                    if files[i].lower().endswith(compression[j]): 
                        print("Compression Found")
                        # Folder Creation
                        try: 
                            os.mkdir("Compressions")
                        except: 
                            pass
                        try: 
                            os.rename(files[i], f"Compressions\\{files[i]}")
                        except: 
                            pass
                except: 
                    pass

                # Apps
                try: 
                    if files[i].lower().endswith(app[j]): 
                        print("App Found")
                        # Folder Creation
                        try: 
                            os.mkdir("Apps")
                        except: 
                            pass
                        try: 
                            os.rename(files[i], f"Apps\\{files[i]}")
                        except: 
                            pass
                except: 
                    pass
            files_no -= 1
            status_bar.set(f"Categorising | Files left = {files_no}")
            status.update()
            main.update()
            
def apply_to_sub_folders(*awargs): 
    pass

def start(*awargs): 
    try: 
        os.chdir(folder_location.get())
        selection = True
    except: 
        showerror("Error! ", "Location not found! Please choose a valid location...")
    status_bar.set("Working on it...")
    status.update()
    
    # Extras
    print(capitalize_files_var.get())
    print(handle_casing_mistakes_var.get())
    print(handle_spacing_mistakes_var.get())
    print(apply_to_sub_folders_var.get())

    print(files_left)
    file = open(".ignore.info", "a+")
    __TEMP__FILES__ = os.listdir(folder_location.get()).remove(".ignore.info")
    for i in files_left:
        file.write(f"{__TEMP__FILES__[i]}\n")
    file.close()
    del __TEMP__FILES__

    if capitalize_files_var.get() == 1: 
        capitalize_files()
    if handle_casing_mistakes_var.get() == 1: 
        handle_casing_mistakes()
    if handle_spacing_mistakes_var.get() == 1: 
        handle_spacing_mistakes()
    if apply_to_sub_folders_var.get() == 1: 
        apply_to_sub_folders()
    if categorise_files_in_folders_var.get() == 1: 
        categorise_files_in_folders()

    status_bar.set("Ready! ")
    status.update()

    try: 
        if selection:
            print("\nDone!\n")
            # os.remove(f"{folder_location.get()}/.ignore.info")
            showinfo("Done! ", "Your folder has successfully Prettified and looks great now.")
            os.startfile(folder_location.get())
    except: 
        pass

# Variables
folder_location = StringVar()
status_bar = StringVar()
files_no_label_label_var = IntVar()

capitalize_files_var = IntVar()
handle_casing_mistakes_var = IntVar()
handle_spacing_mistakes_var = IntVar()
categorise_files_in_folders_var = IntVar()
apply_to_sub_folders_var = IntVar()

style = ttk.Style()

frame_label = Frame(main)
frame_label.pack()
Label(frame_label, text="Enter/Choose the folder location", font=("google sans bold", 15), fg="red").pack()

frame_1 = Frame(main)
frame_1.pack()

ttk.Entry(frame_1, textvariable=folder_location).grid(row=1, column=0)
Label(frame_1, text=" ").grid(row=1, column=1)
ttk.Button(frame_1, text="...", command=get_folder).grid(row=1, column=2)
Label(frame_1, text=" ").grid(row=1, column=3)
ttk.Button(frame_1, text="Current location", command=lambda: get_folder("current")).grid(row=1, column=4)
Label(frame_1, text=" ").grid(row=1, column=5)

Label(main, text=" ").pack()

frame_2 = Frame(main)
frame_2.pack()

def removeFromList():
    clicked = l.curselection()
    for item in clicked:
        files_left.append(item)
    for poss in files_left:
        l.delete(poss)

l = Listbox(frame_2, width=120, height=10, selectmode=MULTIPLE)
l.pack(side="left")
scrollbar = Scrollbar(frame_2, orient="vertical")
scrollbar.config(command=l.yview)
scrollbar.pack(side="right", fill="y")

l.config(yscrollcommand=scrollbar.set)

frame_3 = Frame(main)
frame_3.pack(fill=X)


Label(frame_3, text=" ").grid(row=1)
Label(frame_3, text="\t\t\t\t\t\t\t    ").grid(row=2, column=1)
ttk.Button(frame_3, text="Exclude Files", command=removeFromList).grid(row=2, column=2)

Label(main, text=" ").pack()

frame_4 = Frame(main)
frame_4.pack(fill=X)

style.configure("TCheckbutton", font=("google sans regular", 11))
ttk.Checkbutton(frame_4, style="TCheckbutton", text="Capitalize and remove underscores from name of files", variable=capitalize_files_var, onvalue=1, offvalue=0).pack(anchor=W)
ttk.Checkbutton(frame_4, style="TCheckbutton", text="Handle casing mistakes in all text files", variable=handle_casing_mistakes_var, onvalue=1, offvalue=0).pack(anchor=W)
ttk.Checkbutton(frame_4, style="TCheckbutton", text="Handle all the spacing mistakes after fullstops, commas, colons etc...", variable=handle_spacing_mistakes_var, onvalue=1, offvalue=0).pack(anchor=W)
ttk.Checkbutton(frame_4, style="TCheckbutton", text="Categories files in different folders.", variable=categorise_files_in_folders_var, onvalue=1, offvalue=0).pack(anchor=W)
# ttk.Checkbutton(frame_4, style="TCheckbutton", text="Apply changes to all the sub-folders", variable=apply_to_sub_folders_var, onvalue=1, offvalue=0).pack(anchor=W)

Label(frame_4, text="\n").pack()

ttk.Button(frame_4, text="Prettify", command=start).pack()

Label(frame_4, text="\n").pack()

status = Label(frame_4, textvariable=status_bar, bd=1, relief=SUNKEN, anchor= W)
status.pack(fill=X, side=BOTTOM)

status_bar.set("Ready! ")

main.grab_set()




main.mainloop()
