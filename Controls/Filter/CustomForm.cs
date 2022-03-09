using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataGridViewAutoFilter
{
    public class CustomForm : Form
    {
        private IContainer components;
        private string field;
        private Label labelField;
        private ComboBox comboBoxOperator;
        private TextBox textValue;
        private Button okButton;
        private Button cancelButton;
        private ComboBox languageSelector;
        private Label languageLabel;

        public string Field
        {
            set => field = value;
        }

        public string FieldName
        {
            set => labelField.Text = value;
        }

        public string Filter
        {
            get
            {
                string result = "";

                if (string.IsNullOrEmpty(textValue.Text))
                {
                    field = "";
                }
                else
                {
                    switch (comboBoxOperator.SelectedIndex)
                    {
                        case 0:
                            result = string.Format("[{0}]='{1}'", field, textValue.Text);
                            break;

                        case 1:
                            result = string.Format("[{0}]<>'{1}'", field, textValue.Text);
                            break;

                        case 2:
                            result = string.Format("[{0}]>'{1}'", field, textValue.Text);
                            break;

                        case 3:
                            result = string.Format("[{0}]>='{1}'", field, textValue.Text);
                            break;

                        case 4:
                            result = string.Format("[{0}]<'{1}'", field, textValue.Text);
                            break;

                        case 5:
                            result = string.Format("[{0}]<='{1}'", field, textValue.Text);
                            break;

                        case 6:
                            result = string.Format("Convert({0}, 'System.String') like '%{1}%'", field, textValue.Text);
                            break;

                        case 7:
                            result = string.Format("Convert({0}, 'System.String') not like '%{1}%'", field, textValue.Text);
                            break;
                    }
                }
                return result;
            }
        }

        public CustomForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            LanguageHandler.handler.CheckUserChoice();

            labelField = new Label();
            comboBoxOperator = new ComboBox();
            textValue = new TextBox();
            okButton = new Button();
            cancelButton = new Button();
            languageSelector = new ComboBox();
            languageLabel = new Label();
            components = new Container();
            SuspendLayout();

            // labelField
            //
            labelField.AutoSize = true;
            labelField.Location = new Point(13, 21);
            labelField.Name = "labelField";
            labelField.Text = "Field Name"; // LanguageHandler.resourceManager.GetString("fieldName");
            labelField.Size = new Size(0, 13);
            labelField.TabIndex = 0;
            //
            // comboBoxOperator
            //
            comboBoxOperator.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxOperator.FormattingEnabled = true;
            comboBoxOperator.Items.AddRange(new object[] {
                "Equals",
                "NotEquals",
                "GreaterThan",
                "GreaterThanEquals",
                "LessThan",
                "LessThanEquals",
                "Contains",
                "NotContains"
            //LanguageHandler.resourceManager.GetString("operatorEquals"),
            //LanguageHandler.resourceManager.GetString("operatorNotEquals"),
            //LanguageHandler.resourceManager.GetString("operatorGreaterThan"),
            //LanguageHandler.resourceManager.GetString("operatorGreaterThanEquals"),
            //LanguageHandler.resourceManager.GetString("operatorLessThan"),
            //LanguageHandler.resourceManager.GetString("operatorLessThanEquals"),
            //LanguageHandler.resourceManager.GetString("operatorContains"),
            //LanguageHandler.resourceManager.GetString("operatorNotContains")
            });
            comboBoxOperator.Location = new Point(14, 48);
            comboBoxOperator.Name = "comboBoxOperator";
            comboBoxOperator.Size = new Size(180, 21);
            comboBoxOperator.TabIndex = 1;
            //
            // textValue
            //
            textValue.Location = new Point(214, 47);
            textValue.Name = "textValue";
            textValue.Size = new Size(180, 20);
            textValue.TabIndex = 2;
            //
            // okButton
            //
            okButton.DialogResult = DialogResult.OK;
            okButton.Location = new Point(119, 92);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 25);
            okButton.TabIndex = 3;
            okButton.Text = "Filter"; // LanguageHandler.resourceManager.GetString("determine");
            okButton.UseVisualStyleBackColor = true;
            //
            // cancelButton
            //
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(214, 92);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 25);
            cancelButton.TabIndex = 4;
            cancelButton.Text = "Cancel"; // LanguageHandler.resourceManager.GetString("cancel");
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += CancelButton_Click;
            //
            // CustomForm
            //
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(413, 140);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(textValue);
            Controls.Add(comboBoxOperator);
            Controls.Add(labelField);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CustomForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Customize"; // LanguageHandler.resourceManager.GetString("customize");
            ResumeLayout(false);
            PerformLayout();
        }

        private void CancelButton_Click(object sender, EventArgs e) => Close();

        private void LanguageSelector_SelectionChangeCommitted(object sender, EventArgs e)
        {
            switch (languageSelector.SelectedIndex)
            {
                case 0: LanguageHandler.userChoice = "ar"; break;
                case 1: LanguageHandler.userChoice = "ur"; break;
                case 2: LanguageHandler.userChoice = "es"; break;
                case 3: LanguageHandler.userChoice = "ja"; break;
                case 4: LanguageHandler.userChoice = "zh"; break;
                case 5: LanguageHandler.userChoice = "en"; break;
                case 6: LanguageHandler.userChoice = "ru"; break;
            }

            //Properties.Settings.Default.Language = LanguageHandler.userChoice;
            //Properties.Settings.Default.Save();

            //_ = new LanguageDialog().ShowDialog();
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            switch (ModifierKeys)
            {
                case Keys.None when keyData == Keys.Escape:
                    Close();
                    return true;

                default:
                    return base.ProcessDialogKey(keyData);
            }
        }
    }
}