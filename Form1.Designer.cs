namespace CVG
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            labelCoords = new Label();
            listBox1 = new ListBox();
            addButton = new Button();
            editButton = new Button();
            deleteButton = new Button();
            buttonShow = new Button();
            labelAxis2 = new Label();
            labelAxis3 = new Label();
            labelAxis2Value = new Label();
            labelAxis3Value = new Label();
            buttonWatch = new Button();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(713, 418);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "test";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // labelCoords
            // 
            labelCoords.AutoSize = true;
            labelCoords.Location = new Point(12, 426);
            labelCoords.Name = "labelCoords";
            labelCoords.Size = new Size(26, 15);
            labelCoords.TabIndex = 1;
            labelCoords.Text = "test";
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(12, 12);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(371, 184);
            listBox1.TabIndex = 3;
            // 
            // addButton
            // 
            addButton.Location = new Point(12, 202);
            addButton.Name = "addButton";
            addButton.Size = new Size(52, 23);
            addButton.TabIndex = 4;
            addButton.Text = "Add";
            addButton.UseVisualStyleBackColor = true;
            addButton.Click += addButton_Click;
            // 
            // editButton
            // 
            editButton.Location = new Point(70, 202);
            editButton.Name = "editButton";
            editButton.Size = new Size(61, 23);
            editButton.TabIndex = 5;
            editButton.Text = "Edit";
            editButton.UseVisualStyleBackColor = true;
            editButton.Click += editButton_Click;
            // 
            // deleteButton
            // 
            deleteButton.AutoSize = true;
            deleteButton.Location = new Point(137, 202);
            deleteButton.Name = "deleteButton";
            deleteButton.Size = new Size(52, 25);
            deleteButton.TabIndex = 6;
            deleteButton.Text = "Delete";
            deleteButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            deleteButton.UseVisualStyleBackColor = true;
            deleteButton.Click += deleteButton_Click;
            // 
            // buttonShow
            // 
            buttonShow.Location = new Point(195, 202);
            buttonShow.Name = "buttonShow";
            buttonShow.Size = new Size(59, 23);
            buttonShow.TabIndex = 7;
            buttonShow.Text = "Show";
            buttonShow.UseVisualStyleBackColor = true;
            buttonShow.Click += buttonShow_Click;
            // 
            // labelAxis2
            // 
            labelAxis2.AutoSize = true;
            labelAxis2.Location = new Point(428, 12);
            labelAxis2.Name = "labelAxis2";
            labelAxis2.Size = new Size(40, 15);
            labelAxis2.TabIndex = 8;
            labelAxis2.Text = "Axis 2:";
            // 
            // labelAxis3
            // 
            labelAxis3.AutoSize = true;
            labelAxis3.Location = new Point(552, 12);
            labelAxis3.Name = "labelAxis3";
            labelAxis3.Size = new Size(40, 15);
            labelAxis3.TabIndex = 9;
            labelAxis3.Text = "Axis 3:";
            // 
            // labelAxis2Value
            // 
            labelAxis2Value.AutoSize = true;
            labelAxis2Value.Location = new Point(428, 41);
            labelAxis2Value.Name = "labelAxis2Value";
            labelAxis2Value.Size = new Size(0, 15);
            labelAxis2Value.TabIndex = 10;
            // 
            // labelAxis3Value
            // 
            labelAxis3Value.AutoSize = true;
            labelAxis3Value.Location = new Point(552, 41);
            labelAxis3Value.Name = "labelAxis3Value";
            labelAxis3Value.Size = new Size(0, 15);
            labelAxis3Value.TabIndex = 11;
            // 
            // buttonWatch
            // 
            buttonWatch.Location = new Point(713, 12);
            buttonWatch.Name = "buttonWatch";
            buttonWatch.Size = new Size(75, 23);
            buttonWatch.TabIndex = 12;
            buttonWatch.Text = "Watch axis";
            buttonWatch.UseVisualStyleBackColor = true;
            buttonWatch.Click += async (sender, e) => await buttonWatch_ClickAsync(sender, e);
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(buttonWatch);
            Controls.Add(labelAxis3Value);
            Controls.Add(labelAxis2Value);
            Controls.Add(labelAxis3);
            Controls.Add(labelAxis2);
            Controls.Add(buttonShow);
            Controls.Add(deleteButton);
            Controls.Add(editButton);
            Controls.Add(addButton);
            Controls.Add(listBox1);
            Controls.Add(labelCoords);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Label labelCoords;
        private ListBox listBox1;
        private Button addButton;
        private Button editButton;
        private Button deleteButton;
        private Button buttonShow;
        private Label labelAxis2;
        private Label labelAxis3;
        private Label labelAxis2Value;
        private Label labelAxis3Value;
        private Button buttonWatch;
    }
}
