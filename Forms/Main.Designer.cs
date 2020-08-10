namespace FolderPrettifier
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.folderOptionsGroup = new System.Windows.Forms.GroupBox();
            this.totalFilesCount = new System.Windows.Forms.Label();
            this.totalFilesLabel = new System.Windows.Forms.Label();
            this.renameTo = new System.Windows.Forms.TextBox();
            this.location = new System.Windows.Forms.TextBox();
            this.renameToLabel = new System.Windows.Forms.Label();
            this.locationLabel = new System.Windows.Forms.Label();
            this.chooseLocation = new System.Windows.Forms.Button();
            this.folderActionsGroup = new System.Windows.Forms.GroupBox();
            this.isCategorizeFiles = new System.Windows.Forms.CheckBox();
            this.nameEndsWith = new System.Windows.Forms.TextBox();
            this.nameEndsWithLabel = new System.Windows.Forms.Label();
            this.nameStartsWith = new System.Windows.Forms.TextBox();
            this.nameStartsWithLabel = new System.Windows.Forms.Label();
            this.isNameWith = new System.Windows.Forms.CheckBox();
            this.isHandleSpacing = new System.Windows.Forms.CheckBox();
            this.isPrettifyFile = new System.Windows.Forms.CheckBox();
            this.withWord = new System.Windows.Forms.TextBox();
            this.withWordLabel = new System.Windows.Forms.Label();
            this.replaceWord = new System.Windows.Forms.TextBox();
            this.replaceWordLabel = new System.Windows.Forms.Label();
            this.isReplaceWord = new System.Windows.Forms.CheckBox();
            this.isCapitalizeName = new System.Windows.Forms.CheckBox();
            this.isPrettifyName = new System.Windows.Forms.CheckBox();
            this.startBtn = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.status = new System.Windows.Forms.Label();
            this.updateCatalogBtn = new System.Windows.Forms.Button();
            this.aboutBtn = new System.Windows.Forms.Button();
            this.isOpenFolder = new System.Windows.Forms.CheckBox();
            this.folderOptionsGroup.SuspendLayout();
            this.folderActionsGroup.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // folderOptionsGroup
            // 
            this.folderOptionsGroup.Controls.Add(this.totalFilesCount);
            this.folderOptionsGroup.Controls.Add(this.totalFilesLabel);
            this.folderOptionsGroup.Controls.Add(this.renameTo);
            this.folderOptionsGroup.Controls.Add(this.location);
            this.folderOptionsGroup.Controls.Add(this.renameToLabel);
            this.folderOptionsGroup.Controls.Add(this.locationLabel);
            this.folderOptionsGroup.Controls.Add(this.chooseLocation);
            this.folderOptionsGroup.Location = new System.Drawing.Point(12, 12);
            this.folderOptionsGroup.Name = "folderOptionsGroup";
            this.folderOptionsGroup.Size = new System.Drawing.Size(378, 106);
            this.folderOptionsGroup.TabIndex = 0;
            this.folderOptionsGroup.TabStop = false;
            this.folderOptionsGroup.Text = "Folder Options";
            // 
            // totalFilesCount
            // 
            this.totalFilesCount.AutoSize = true;
            this.totalFilesCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.totalFilesCount.Location = new System.Drawing.Point(56, 84);
            this.totalFilesCount.Name = "totalFilesCount";
            this.totalFilesCount.Size = new System.Drawing.Size(21, 13);
            this.totalFilesCount.TabIndex = 11;
            this.totalFilesCount.Text = "34";
            // 
            // totalFilesLabel
            // 
            this.totalFilesLabel.AutoSize = true;
            this.totalFilesLabel.Location = new System.Drawing.Point(6, 84);
            this.totalFilesLabel.Name = "totalFilesLabel";
            this.totalFilesLabel.Size = new System.Drawing.Size(55, 13);
            this.totalFilesLabel.TabIndex = 10;
            this.totalFilesLabel.Text = "Total files:";
            // 
            // renameTo
            // 
            this.renameTo.Location = new System.Drawing.Point(78, 47);
            this.renameTo.Name = "renameTo";
            this.renameTo.Size = new System.Drawing.Size(286, 20);
            this.renameTo.TabIndex = 2;
            this.renameTo.TextChanged += new System.EventHandler(this.renameTo_TextChanged);
            // 
            // location
            // 
            this.location.Location = new System.Drawing.Point(78, 22);
            this.location.Name = "location";
            this.location.Size = new System.Drawing.Size(256, 20);
            this.location.TabIndex = 0;
            this.location.TextChanged += new System.EventHandler(this.location_TextChanged);
            // 
            // renameToLabel
            // 
            this.renameToLabel.AutoSize = true;
            this.renameToLabel.Location = new System.Drawing.Point(6, 51);
            this.renameToLabel.Name = "renameToLabel";
            this.renameToLabel.Size = new System.Drawing.Size(63, 13);
            this.renameToLabel.TabIndex = 8;
            this.renameToLabel.Text = "Rename To";
            // 
            // locationLabel
            // 
            this.locationLabel.AutoSize = true;
            this.locationLabel.Location = new System.Drawing.Point(6, 25);
            this.locationLabel.Name = "locationLabel";
            this.locationLabel.Size = new System.Drawing.Size(48, 13);
            this.locationLabel.TabIndex = 5;
            this.locationLabel.Text = "Location";
            // 
            // chooseLocation
            // 
            this.chooseLocation.Location = new System.Drawing.Point(340, 22);
            this.chooseLocation.Name = "chooseLocation";
            this.chooseLocation.Size = new System.Drawing.Size(24, 20);
            this.chooseLocation.TabIndex = 1;
            this.chooseLocation.Text = "...";
            this.chooseLocation.UseVisualStyleBackColor = true;
            this.chooseLocation.Click += new System.EventHandler(this.chooseLocation_Click);
            // 
            // folderActionsGroup
            // 
            this.folderActionsGroup.Controls.Add(this.isCategorizeFiles);
            this.folderActionsGroup.Controls.Add(this.nameEndsWith);
            this.folderActionsGroup.Controls.Add(this.nameEndsWithLabel);
            this.folderActionsGroup.Controls.Add(this.nameStartsWith);
            this.folderActionsGroup.Controls.Add(this.nameStartsWithLabel);
            this.folderActionsGroup.Controls.Add(this.isNameWith);
            this.folderActionsGroup.Controls.Add(this.isHandleSpacing);
            this.folderActionsGroup.Controls.Add(this.isPrettifyFile);
            this.folderActionsGroup.Controls.Add(this.withWord);
            this.folderActionsGroup.Controls.Add(this.withWordLabel);
            this.folderActionsGroup.Controls.Add(this.replaceWord);
            this.folderActionsGroup.Controls.Add(this.replaceWordLabel);
            this.folderActionsGroup.Controls.Add(this.isReplaceWord);
            this.folderActionsGroup.Controls.Add(this.isCapitalizeName);
            this.folderActionsGroup.Controls.Add(this.isPrettifyName);
            this.folderActionsGroup.Location = new System.Drawing.Point(12, 124);
            this.folderActionsGroup.Name = "folderActionsGroup";
            this.folderActionsGroup.Size = new System.Drawing.Size(378, 292);
            this.folderActionsGroup.TabIndex = 1;
            this.folderActionsGroup.TabStop = false;
            this.folderActionsGroup.Text = "Actions";
            // 
            // isCategorizeFiles
            // 
            this.isCategorizeFiles.AutoSize = true;
            this.isCategorizeFiles.Location = new System.Drawing.Point(9, 219);
            this.isCategorizeFiles.Name = "isCategorizeFiles";
            this.isCategorizeFiles.Size = new System.Drawing.Size(100, 17);
            this.isCategorizeFiles.TabIndex = 16;
            this.isCategorizeFiles.Text = "Categorize Files";
            this.isCategorizeFiles.UseVisualStyleBackColor = true;
            // 
            // nameEndsWith
            // 
            this.nameEndsWith.Enabled = false;
            this.nameEndsWith.Location = new System.Drawing.Point(110, 191);
            this.nameEndsWith.Name = "nameEndsWith";
            this.nameEndsWith.Size = new System.Drawing.Size(254, 20);
            this.nameEndsWith.TabIndex = 11;
            this.nameEndsWith.TextChanged += new System.EventHandler(this.nameEndsWith_TextChanged);
            // 
            // nameEndsWithLabel
            // 
            this.nameEndsWithLabel.AutoSize = true;
            this.nameEndsWithLabel.Enabled = false;
            this.nameEndsWithLabel.Location = new System.Drawing.Point(45, 194);
            this.nameEndsWithLabel.Name = "nameEndsWithLabel";
            this.nameEndsWithLabel.Size = new System.Drawing.Size(56, 13);
            this.nameEndsWithLabel.TabIndex = 16;
            this.nameEndsWithLabel.Text = "Ends With";
            // 
            // nameStartsWith
            // 
            this.nameStartsWith.Enabled = false;
            this.nameStartsWith.Location = new System.Drawing.Point(110, 165);
            this.nameStartsWith.Name = "nameStartsWith";
            this.nameStartsWith.Size = new System.Drawing.Size(254, 20);
            this.nameStartsWith.TabIndex = 10;
            this.nameStartsWith.TextChanged += new System.EventHandler(this.nameStartsWith_TextChanged);
            // 
            // nameStartsWithLabel
            // 
            this.nameStartsWithLabel.AutoSize = true;
            this.nameStartsWithLabel.Enabled = false;
            this.nameStartsWithLabel.Location = new System.Drawing.Point(45, 168);
            this.nameStartsWithLabel.Name = "nameStartsWithLabel";
            this.nameStartsWithLabel.Size = new System.Drawing.Size(59, 13);
            this.nameStartsWithLabel.TabIndex = 14;
            this.nameStartsWithLabel.Text = "Starts With";
            // 
            // isNameWith
            // 
            this.isNameWith.AutoSize = true;
            this.isNameWith.Enabled = false;
            this.isNameWith.Location = new System.Drawing.Point(29, 142);
            this.isNameWith.Name = "isNameWith";
            this.isNameWith.Size = new System.Drawing.Size(79, 17);
            this.isNameWith.TabIndex = 9;
            this.isNameWith.Text = "Name With";
            this.isNameWith.UseVisualStyleBackColor = true;
            this.isNameWith.CheckedChanged += new System.EventHandler(this.isNameWith_CheckedChanged);
            // 
            // isHandleSpacing
            // 
            this.isHandleSpacing.AutoSize = true;
            this.isHandleSpacing.Enabled = false;
            this.isHandleSpacing.Location = new System.Drawing.Point(29, 265);
            this.isHandleSpacing.Name = "isHandleSpacing";
            this.isHandleSpacing.Size = new System.Drawing.Size(272, 17);
            this.isHandleSpacing.TabIndex = 15;
            this.isHandleSpacing.Text = "Fix spacing mistakes after punctuations (eg: comma)";
            this.isHandleSpacing.UseVisualStyleBackColor = true;
            // 
            // isPrettifyFile
            // 
            this.isPrettifyFile.AutoSize = true;
            this.isPrettifyFile.Location = new System.Drawing.Point(9, 242);
            this.isPrettifyFile.Name = "isPrettifyFile";
            this.isPrettifyFile.Size = new System.Drawing.Size(114, 17);
            this.isPrettifyFile.TabIndex = 14;
            this.isPrettifyFile.Text = "Pretify File Content";
            this.isPrettifyFile.UseVisualStyleBackColor = true;
            this.isPrettifyFile.CheckedChanged += new System.EventHandler(this.isPrettifyFile_CheckedChanged);
            // 
            // withWord
            // 
            this.withWord.Enabled = false;
            this.withWord.Location = new System.Drawing.Point(99, 116);
            this.withWord.Name = "withWord";
            this.withWord.Size = new System.Drawing.Size(265, 20);
            this.withWord.TabIndex = 8;
            this.withWord.TextChanged += new System.EventHandler(this.withWord_TextChanged);
            // 
            // withWordLabel
            // 
            this.withWordLabel.AutoSize = true;
            this.withWordLabel.Enabled = false;
            this.withWordLabel.Location = new System.Drawing.Point(45, 119);
            this.withWordLabel.Name = "withWordLabel";
            this.withWordLabel.Size = new System.Drawing.Size(29, 13);
            this.withWordLabel.TabIndex = 6;
            this.withWordLabel.Text = "With";
            // 
            // replaceWord
            // 
            this.replaceWord.Enabled = false;
            this.replaceWord.Location = new System.Drawing.Point(99, 90);
            this.replaceWord.Name = "replaceWord";
            this.replaceWord.Size = new System.Drawing.Size(265, 20);
            this.replaceWord.TabIndex = 7;
            this.replaceWord.TextChanged += new System.EventHandler(this.replaceWord_TextChanged);
            // 
            // replaceWordLabel
            // 
            this.replaceWordLabel.AutoSize = true;
            this.replaceWordLabel.Enabled = false;
            this.replaceWordLabel.Location = new System.Drawing.Point(45, 93);
            this.replaceWordLabel.Name = "replaceWordLabel";
            this.replaceWordLabel.Size = new System.Drawing.Size(47, 13);
            this.replaceWordLabel.TabIndex = 4;
            this.replaceWordLabel.Text = "Replace";
            // 
            // isReplaceWord
            // 
            this.isReplaceWord.AutoSize = true;
            this.isReplaceWord.Enabled = false;
            this.isReplaceWord.Location = new System.Drawing.Point(29, 66);
            this.isReplaceWord.Name = "isReplaceWord";
            this.isReplaceWord.Size = new System.Drawing.Size(136, 17);
            this.isReplaceWord.TabIndex = 6;
            this.isReplaceWord.Text = "Replace Specific Word";
            this.isReplaceWord.UseVisualStyleBackColor = true;
            this.isReplaceWord.CheckedChanged += new System.EventHandler(this.isReplaceWord_CheckedChanged);
            // 
            // isCapitalizeName
            // 
            this.isCapitalizeName.AutoSize = true;
            this.isCapitalizeName.Enabled = false;
            this.isCapitalizeName.Location = new System.Drawing.Point(29, 43);
            this.isCapitalizeName.Name = "isCapitalizeName";
            this.isCapitalizeName.Size = new System.Drawing.Size(71, 17);
            this.isCapitalizeName.TabIndex = 4;
            this.isCapitalizeName.Text = "Capitalize";
            this.isCapitalizeName.UseVisualStyleBackColor = true;
            // 
            // isPrettifyName
            // 
            this.isPrettifyName.AutoSize = true;
            this.isPrettifyName.Location = new System.Drawing.Point(9, 20);
            this.isPrettifyName.Name = "isPrettifyName";
            this.isPrettifyName.Size = new System.Drawing.Size(89, 17);
            this.isPrettifyName.TabIndex = 3;
            this.isPrettifyName.Text = "Prettify Name";
            this.isPrettifyName.UseVisualStyleBackColor = true;
            this.isPrettifyName.CheckedChanged += new System.EventHandler(this.isPrettifyName_CheckedChanged);
            // 
            // startBtn
            // 
            this.startBtn.Location = new System.Drawing.Point(12, 478);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(69, 28);
            this.startBtn.TabIndex = 17;
            this.startBtn.Text = "Start";
            this.startBtn.UseVisualStyleBackColor = true;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel1.Controls.Add(this.progressBar);
            this.panel1.Controls.Add(this.status);
            this.panel1.Location = new System.Drawing.Point(1, 516);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(401, 25);
            this.panel1.TabIndex = 2;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(233, 6);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(157, 13);
            this.progressBar.TabIndex = 4;
            // 
            // status
            // 
            this.status.AutoSize = true;
            this.status.Location = new System.Drawing.Point(7, 6);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(38, 13);
            this.status.TabIndex = 0;
            this.status.Text = "Ready";
            // 
            // updateCatalogBtn
            // 
            this.updateCatalogBtn.Location = new System.Drawing.Point(90, 478);
            this.updateCatalogBtn.Name = "updateCatalogBtn";
            this.updateCatalogBtn.Size = new System.Drawing.Size(114, 28);
            this.updateCatalogBtn.TabIndex = 18;
            this.updateCatalogBtn.Text = "Update Catalog";
            this.updateCatalogBtn.UseVisualStyleBackColor = true;
            this.updateCatalogBtn.Click += new System.EventHandler(this.updateCatalogBtn_Click);
            // 
            // aboutBtn
            // 
            this.aboutBtn.Location = new System.Drawing.Point(331, 478);
            this.aboutBtn.Name = "aboutBtn";
            this.aboutBtn.Size = new System.Drawing.Size(59, 28);
            this.aboutBtn.TabIndex = 19;
            this.aboutBtn.Text = "About";
            this.aboutBtn.UseVisualStyleBackColor = true;
            this.aboutBtn.Click += new System.EventHandler(this.aboutBtn_Click);
            // 
            // isOpenFolder
            // 
            this.isOpenFolder.AutoSize = true;
            this.isOpenFolder.Location = new System.Drawing.Point(12, 432);
            this.isOpenFolder.Name = "isOpenFolder";
            this.isOpenFolder.Size = new System.Drawing.Size(165, 17);
            this.isOpenFolder.TabIndex = 20;
            this.isOpenFolder.Text = "Open folder after prettification";
            this.isOpenFolder.UseVisualStyleBackColor = true;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(401, 540);
            this.Controls.Add(this.isOpenFolder);
            this.Controls.Add(this.aboutBtn);
            this.Controls.Add(this.updateCatalogBtn);
            this.Controls.Add(this.startBtn);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.folderActionsGroup);
            this.Controls.Add(this.folderOptionsGroup);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Folder Prettifier";
            this.folderOptionsGroup.ResumeLayout(false);
            this.folderOptionsGroup.PerformLayout();
            this.folderActionsGroup.ResumeLayout(false);
            this.folderActionsGroup.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox folderOptionsGroup;
        private System.Windows.Forms.TextBox renameTo;
        private System.Windows.Forms.TextBox location;
        private System.Windows.Forms.Label renameToLabel;
        private System.Windows.Forms.Label locationLabel;
        private System.Windows.Forms.Button chooseLocation;
        private System.Windows.Forms.Label totalFilesLabel;
        private System.Windows.Forms.Label totalFilesCount;
        private System.Windows.Forms.GroupBox folderActionsGroup;
        private System.Windows.Forms.Label withWordLabel;
        private System.Windows.Forms.TextBox replaceWord;
        private System.Windows.Forms.Label replaceWordLabel;
        private System.Windows.Forms.CheckBox isReplaceWord;
        private System.Windows.Forms.CheckBox isCapitalizeName;
        private System.Windows.Forms.CheckBox isPrettifyName;
        private System.Windows.Forms.CheckBox isPrettifyFile;
        private System.Windows.Forms.CheckBox isHandleSpacing;
        private System.Windows.Forms.CheckBox isCategorizeFiles;
        private System.Windows.Forms.TextBox nameEndsWith;
        private System.Windows.Forms.Label nameEndsWithLabel;
        private System.Windows.Forms.TextBox nameStartsWith;
        private System.Windows.Forms.Label nameStartsWithLabel;
        private System.Windows.Forms.CheckBox isNameWith;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox withWord;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button updateCatalogBtn;
        private System.Windows.Forms.Button aboutBtn;
        private System.Windows.Forms.CheckBox isOpenFolder;
    }
}

