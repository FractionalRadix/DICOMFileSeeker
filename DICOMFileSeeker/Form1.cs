using Dicom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DICOMFileSeeker
{
    //TODO!+
    // Two main tasks still need to be done.
    // (1) The actual search must be made asynchronous - DONE!
    // (2) The DICOM Tag Input controls must allow the user to specify a value.
    // ...After this, perhaps we could make a cache of files with their dicom tags and timestamps.

    public partial class Form1 : Form
    {
        private TextBox tbDirectoryToSearch;
        private TextBox tbSearchResults;
        private Label lblStatusMessage;
        private DicomTagInputControl tagInput1, tagInput2, tagInput3;

        public Form1()
        {
            InitializeComponent();
            this.Text = "DICOM File Seeker";
            AddControls();
        }

        /// <summary>
        /// .NET Core 3 does not yet include a Forms Builder.
        /// So instead of adding the controls on a Forms Builder, we add them using code.
        /// </summary>
        void AddControls()
        {
            // x-coordinate of the leftmost controls.
            int leftStart = 20;

            Label lblDirectoryToSearch = new Label
            {
                Text = "Directory to search:",
                Location = new Point(leftStart, 20),
                Size = new Size(400, 20)
            };
            this.Controls.Add(lblDirectoryToSearch);

            tbDirectoryToSearch = new TextBox
            {
                Location = new Point(leftStart, 40),
                Size = new Size(600, 20)
            };
            this.Controls.Add(tbDirectoryToSearch);

            Button btStartSearching = new Button()
            {
                Location = new Point(630, 40),
                Size = new Size(100, 20),
                Text = "Start searching",
            };
            btStartSearching.Click += BtStartSearching_Click;
            this.Controls.Add(btStartSearching);


            lblStatusMessage = new Label
            {
                Location = new Point(leftStart, 70),
                Size = new Size(700, 20),
                ForeColor = Color.Black,
                Text = "Status: waiting for input."
            };
            this.Controls.Add(lblStatusMessage);

            int leftSideOfTagBox = 40;
            int topOfTagBox = 100;

            tagInput1 = new DicomTagInputControl(this, leftSideOfTagBox, topOfTagBox);
            tagInput2 = new DicomTagInputControl(this, leftSideOfTagBox, topOfTagBox + 100);
            tagInput3 = new DicomTagInputControl(this, leftSideOfTagBox, topOfTagBox + 200);

            tbSearchResults = new TextBox()
            {
                Location = new Point(280, 100),
                Size = new Size(400, 400),
                Multiline = true,
                Enabled = false,
                ScrollBars = ScrollBars.Both
            };
            this.Controls.Add(tbSearchResults);
        }

        private async void BtStartSearching_Click(object sender, EventArgs e)
        {
            tbSearchResults.Clear();
            string directoryToSearch = tbDirectoryToSearch.Text;

            try
            {
                lblStatusMessage.ForeColor = Color.Black;
                lblStatusMessage.Text = $"Status: searching directory {tbDirectoryToSearch.Text}";

                await DoSearchAsync(directoryToSearch);

                //TODO!+ ONLY set status to "Search complete" if there are no errors.
                // ...or, perhaps, make a separate place for error messages...
                lblStatusMessage.ForeColor = Color.Black;
                lblStatusMessage.Text = "Status: search complete!";
            }
            catch (ArgumentNullException)
            {
                lblStatusMessage.ForeColor = Color.Red;
                lblStatusMessage.Text = "Error: directory to search is not specified.";
            }
            catch (ArgumentOutOfRangeException)
            {
                lblStatusMessage.ForeColor = Color.Red;
                lblStatusMessage.Text = "Error: unclear what directory to search.";
            }
            catch (ArgumentException)
            {
                lblStatusMessage.ForeColor = Color.Red;
                lblStatusMessage.Text = "Error: unclear what directory to search.";
            }
            catch (DirectoryNotFoundException)
            {
                lblStatusMessage.ForeColor = Color.Red;
                lblStatusMessage.Text = $"Error: could not find directory {tbDirectoryToSearch.Text}";
            }
            catch (PathTooLongException)
            {
                lblStatusMessage.ForeColor = Color.Red;
                lblStatusMessage.Text = "Error: directory path too long.";
            }
            catch (IOException)
            {
                lblStatusMessage.ForeColor = Color.Red;
                lblStatusMessage.Text = $"Error: failed to read directory or files at {tbDirectoryToSearch.Text}";
            }
            catch (UnauthorizedAccessException)
            {
                lblStatusMessage.ForeColor = Color.Red;
                lblStatusMessage.Text = $"Error: access refused for directory or files at {tbDirectoryToSearch.Text}";
            }
            catch (SecurityException)
            {
                lblStatusMessage.ForeColor = Color.Red;
                lblStatusMessage.Text = $"Error: security error while reading directory or files at {tbDirectoryToSearch.Text}";
            }
        }


        private async Task DoSearchAsync(string directoryToSearch)
        {
            await Task.Run(() => DoSearch(directoryToSearch));
        }

        private void DoSearch(string directoryToSearch)
        {
            var filenames = Directory.EnumerateFiles(directoryToSearch, "*.dcm", SearchOption.AllDirectories);
            Dicom.DicomTag tag1 = tagInput1.Get();
            Dicom.DicomTag tag2 = tagInput2.Get();
            Dicom.DicomTag tag3 = tagInput3.Get();
            foreach (string filename in filenames)
            {
                //TODO!~ Use OpenAsync instead of Open.
                //TODO!+ If the file is not a DICOM file, DicomFile.Open (and DicomFile.OpenAsync) throws an Exception.
                // We catch that Exception. But if we're using Async, we should add  a "finally" to deal with this.

                try
                {
                    Dicom.DicomFile dicomFile = Dicom.DicomFile.Open(filename, Dicom.FileReadOption.Default); //TODO?~ Make sure we ONLY read the tags! Dicom.FileReadOption comes in here.
                    bool noTagsSpecified = (tag1 == null && tag2 == null && tag3 == null);
                    bool tag1Present = tag1 != null && dicomFile.Dataset.Contains(tag1);
                    bool tag2Present = tag2 != null && dicomFile.Dataset.Contains(tag2);
                    bool tag3Present = tag3 != null && dicomFile.Dataset.Contains(tag3);
                    if (noTagsSpecified || tag1Present || tag2Present || tag3Present)
                    {
                        tbSearchResults.Text += (filename + Environment.NewLine);
                    }
                }
                catch (DicomFileException exc)
                {
                    lblStatusMessage.ForeColor = Color.Red;
                    lblStatusMessage.Text = $"Error: tried to open a file that was not a DICOM file.";
                }
            }
        }
    }

    /// <summary>
    /// Input control for a DICOM Tag.
    /// Not making this a User Defined Control yet.
    /// </summary>
    internal class DicomTagInputControl
    {
        private TextBox tbDicomTagGroup;
        private TextBox tbDicomTagElement;

        /// <summary>
        /// Create a new GroupBox with DICOM Tag controls, and place it on the parent Control.
        /// </summary>
        /// <param name="parent">Parent control for the GroupBox.</param>
        /// <param name="left">Leftmost (x) coordinate of the GroupBox.</param>
        /// <param name="top">Topmost (y) coordinate of the GroupBox.</param>
        internal DicomTagInputControl(Control parent, int left, int top)
        {
            GroupBox gbDicomTag = new GroupBox();
            gbDicomTag.Location = new Point(left, top);
            gbDicomTag.Text = "DICOM Tag";

            // DICOM Tag: group part
            Label lblDicomTagGroup = new Label
            {
                Text = "Group (in hexadecimals):",
                Location = new Point(10, 20),
                Size = new Size(150, 20)
            };
            gbDicomTag.Controls.Add(lblDicomTagGroup);

            tbDicomTagGroup = new TextBox
            {
                Location = new Point(160, 20),
                Size = new Size(40, 20)
            };
            gbDicomTag.Controls.Add(tbDicomTagGroup);

            // DICOM Tag: element part
            Label lblDicomTagElement = new Label
            {
                Text = "Element (in hexadecimals):",
                Location = new Point(10, 50),
                Size = new Size(150, 20)
            };
            gbDicomTag.Controls.Add(lblDicomTagElement);

            tbDicomTagElement = new TextBox
            {
                Location = new Point(160, 50),
                Size = new Size(40, 20)
            };
            gbDicomTag.Controls.Add(tbDicomTagElement);

            //TODO!+ Add a control for the contents that the tag should have.

            // Finally, add the GroupBox to the parent control.
            parent.Controls.Add(gbDicomTag);
        }

        /// <summary>
        /// Get the DICOM tag that the user entered in this control.
        /// If the user did not specify a DICOM tag, or specified an invalid tag (e.g. an Element without a Group,
        /// or an Element or Group value that is not a hexadecimal value) then it returns <code>null</code>.
        /// </summary>
        internal Dicom.DicomTag Get()
        {
            // Parse the Group component of the Tag. If it fails, return null.
            var groupValue = tbDicomTagGroup.Text;
            if (!Int16.TryParse(groupValue, NumberStyles.HexNumber, null, out short group))
                return null;

            // Parse the Element component of the Tag. If it fails, return null.
            var elementValue = tbDicomTagElement.Text;
            if (!Int16.TryParse(elementValue, NumberStyles.HexNumber, null, out short element))
                return null;

            // Return the DICOM tag.
            return new Dicom.DicomTag((ushort) group, (ushort) element);
        }
    }
}
