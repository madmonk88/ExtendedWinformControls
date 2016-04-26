using System.Collections;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System;
using System.Globalization;

namespace ExtendedControls
{
    public class ExtendedControls
    {

        #region SearchBox
        public class SearchBox : TextBox
        {
            private BindingSource sourceCollection = new BindingSource();
            private string displaymember;
            private IComponentChangeService _changeService;
            private Image _clearImage;
            private Image _searchImage;
            private Size _ImageSize;
            private string cue = "Search";
            private CustomButton btn = new CustomButton();

            public override DockStyle Dock
            {
                get
                {
                    return base.Dock;
                }
                set
                {
                    base.Dock = value;
                }
            }

            public override AnchorStyles Anchor
            {
                get
                {
                    return base.Anchor;
                }
                set
                {
                    base.Anchor = value;
                }
            }

            public override System.ComponentModel.ISite Site
            {
                get
                {
                    return base.Site;
                }
                set
                {
                    _changeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
                    if (_changeService != null)
                        _changeService.ComponentChanged -= new ComponentChangedEventHandler(OnComponentChanged);
                    base.Site = value;
                    if (!DesignMode)
                        return;
                    _changeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
                    if (_changeService != null)
                        _changeService.ComponentChanged += new ComponentChangedEventHandler(OnComponentChanged);
                }
            }

            public BindingSource SearchCollection
            {
                get { return sourceCollection; }
                set { sourceCollection = value; }
            }

            public string SearchDisplayMember
            {
                get { return displaymember; }
                set { displaymember = value; }
            }

            protected Size ImageSize
            {
                get { return _ImageSize; }
            }

            public Image SearchImage
            {
                get { return _searchImage; }
                set
                {
                    if (value == null)
                        _ImageSize = Size.Empty;
                    else
                        _ImageSize = value.Size;

                    _searchImage = value;
                    this.Clear();
                    this.Text = this.cue;
                    this.Invalidate();
                }
            }

            public Image ClearImage
            {
                get { return _clearImage; }
                set
                {
                    if (value == null)
                        _ImageSize = Size.Empty;
                    else
                        _ImageSize = value.Size;

                    _clearImage = value;
                }
            }

            [Localizable(true)]
            public string Cue
            {
                get { return cue; }
                set
                {
                    cue = value;
                    this.Text = cue;
                    updateCue();
                }
            }

            private void OnComponentChanged(object sender, ComponentChangedEventArgs ce)
            {
                SearchBox sb = ce.Component as SearchBox;
                if (sb == null || !sb.DesignMode)
                    return;
                if (((IComponent)ce.Component).Site == null || ce.Member == null || ce.Member.Name != "Text")
                    return;
                if (sb.Text == sb.Name)
                    sb.Text = this.cue;
            }

            private void updateCue()
            {
                if (this.IsHandleCreated && cue != null)
                    SendMessage(this.Handle, 0x1501, (IntPtr)1, cue);
            }
            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                updateCue();
            }

            class CustomButton : System.Windows.Forms.Button
            {
                private bool _DisplayFocusCues = false;

                public CustomButton()
                    : base()
                {
                    this.SetStyle(ControlStyles.Selectable, false);
                }

                protected override bool ShowFocusCues
                {
                    get { return _DisplayFocusCues; }
                }

                public bool DisplayFocusCues
                {
                    get { return _DisplayFocusCues; }
                    set { _DisplayFocusCues = value; }
                }
            }

            public SearchBox()
                : base()
            {
                //SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                //SetStyle(ControlStyles.UserPaint, true);

                if (this.cue == null || string.IsNullOrEmpty(this.cue.ToString()))
                    this.cue = "Search";

                this.TextChanged += SearchBox_TextChanged;
                this.GotFocus += (s1, e1) => { if (!string.IsNullOrEmpty(this.Text) && this.Text.Equals(cue)) this.Text = string.Empty; };
                this.Text = this.cue;
                ForeColor = Color.FromArgb(20, 20, 20); // SystemColors.GrayText;
                DoubleBuffered = true;

                btn.AutoSize = true;
                btn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                btn.Dock = DockStyle.Right;
                btn.Cursor = Cursors.Default;
                if (!string.IsNullOrEmpty(this.Text) && !this.Text.Equals(this.cue.ToString()))
                    btn.Image = _clearImage;
                else
                    btn.Image = _searchImage;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Location = new Point(this.ClientSize.Width - btn.Width, -1);
                btn.Click += (s1, e1) => { this.Text = string.Empty; Application.DoEvents(); };
                this.Controls.Add(btn);
            }

            public void SearchBox_TextChanged(object sender, EventArgs e)
            {

                if (sourceCollection != null && !string.IsNullOrEmpty(this.Text) && !this.Text.Equals(cue))
                {
                    this.Invoke((MethodInvoker)delegate
           {
               sourceCollection.Filter = string.Format("[{0}] LIKE '%{1}%'",
                                  displaymember,
                                  this.Text);
           });
                }

                else sourceCollection.Filter = null;

                if (this.Text.Equals(cue))
                    ForeColor = Color.FromArgb(20, 20, 20); // SystemColors.GrayText;
                else
                    ForeColor = SystemColors.WindowText;
                
                if (_searchImage == null && _clearImage == null)
                    return;
                
                if (!string.IsNullOrEmpty(this.Text) && !this.Text.Equals(this.cue.ToString()))
                    btn.Image = _clearImage;
                else
                    btn.Image = _searchImage;
                                
                // Send EM_SETMARGINS to prevent text from disappearing underneath the button
                SendMessage(this.Handle, 0xd3, (IntPtr)2, (IntPtr)(btn.Width << 16));
                Application.DoEvents();
            }
                                    
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, string lp);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        }
        #endregion

        #region ExtendedListBox
        [Docking(DockingBehavior.AutoDock)]
        public class ExtendedListBox : Control
        {
            public SearchBox searchInput = new SearchBox();
            public ListBox listBox = new ListBox();
            public Label headerlbl = new Label();

            private BindingSource sourceCollection = new BindingSource();
            private Image _clearImage;
            private Image _searchImage;
            private string _displaymember;
            private string _headertext = "Header";
            private Color _headerbackcolor = SystemColors.Highlight;
            private Color _headerforecolor = SystemColors.HighlightText;

            public override DockStyle Dock
            {
                get
                {
                    return base.Dock;
                }
                set
                {
                    base.Dock = value;
                }
            }

            public override AnchorStyles Anchor
            {
                get
                {
                    return base.Anchor;
                }
                set
                {
                    base.Anchor = value;
                }
            }

            public string DisplayMember
            {
                get { return _displaymember; }
                set
                {
                    _displaymember = value;
                    searchInput.SearchDisplayMember = _displaymember;
                    listBox.DisplayMember = _displaymember;
                }
            }

            public string HeaderText
            {
                get { return _headertext; }
                set
                {
                    _headertext = value;
                    headerlbl.Text = _headertext;
                }
            }

            public Color HeaderBackColor
            {
                get { return _headerbackcolor; }
                set
                {
                    _headerbackcolor = value;
                    headerlbl.BackColor = _headerbackcolor;
                }
            }

            public Color HeaderForeColor
            {
                get { return _headerforecolor; }
                set
                {
                    _headerforecolor = value;
                    headerlbl.ForeColor = _headerforecolor;
                }
            }

            public BindingSource bindingsource
            {
                get { return sourceCollection; }
                set
                {
                    sourceCollection = value;
                    DataBind();
                }
            }

            private void DataBind()
            {
                listBox.BeginUpdate();
                listBox.Items.Clear();
                searchInput.SearchCollection = sourceCollection;
                listBox.DataSource = sourceCollection;                
                listBox.EndUpdate();
                listBox.Invalidate();
            }

            public Image SearchImage
            {
                get { return _searchImage; }
                set
                {
                    _searchImage = value;
                    searchInput.SearchImage = _searchImage;
                }
            }

            public Image ClearImage
            {
                get { return _clearImage; }
                set
                {
                    _clearImage = value;
                    searchInput.ClearImage = _clearImage;
                }
            }

            public ExtendedListBox()
            {
                //SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                //SetStyle(ControlStyles.UserPaint, true);

                this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                this.SetStyle(ControlStyles.DoubleBuffer, true);
                this.SetStyle(ControlStyles.ResizeRedraw, true);
                this.SetStyle(ControlStyles.Selectable, true);
                this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                this.SetStyle(ControlStyles.UserPaint, true);

                headerlbl.Text = HeaderText;
                headerlbl.AutoSize = true;
                headerlbl.Dock = DockStyle.Fill;
                headerlbl.BackColor = HeaderBackColor;
                headerlbl.ForeColor = HeaderForeColor;
                headerlbl.Margin = new System.Windows.Forms.Padding(0);
                listBox.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);

                headerlbl.Font = new System.Drawing.Font("Calibri", 10, FontStyle.Regular);

                searchInput.BorderStyle = BorderStyle.None;
                listBox.BorderStyle = BorderStyle.None;

                this.BackColor = Color.FromArgb(206, 236, 206);
                searchInput.BackColor = Color.FromArgb(206, 236, 206);
                searchInput.ForeColor = Color.FromArgb(20, 20, 20); // SystemColors.GrayText;
                searchInput.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
                listBox.SelectionMode = SelectionMode.MultiExtended;
                listBox.Dock = DockStyle.Fill;
                listBox.IntegralHeight = false;
                listBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
                listBox.DrawItem += new DrawItemEventHandler(this.DrawItemHandler);

                searchInput.Size = new Size(this.Width, 24);                
                searchInput.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                searchInput.Dock = DockStyle.Fill;
                searchInput.Font = new Font("Segoe UI", 10);
                searchInput.Multiline = false;

                bindingsource.DataSourceChanged += (s1, e1) =>
                {
                    DataBind();
                };
                
                TableLayoutPanel fp = new TableLayoutPanel();
                fp.Dock = DockStyle.Fill;
                fp.AutoSize = true;
                fp.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                fp.ColumnCount = 1;
                fp.RowCount = 3;
                fp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                fp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                fp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                fp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                fp.Controls.Add(headerlbl, 0, 0);
                fp.Controls.Add(searchInput, 0, 1);
                //fp.Controls.Add(tp, 0, 1);
                fp.Controls.Add(listBox, 0, 2);
                fp.Paint += (s1, e) =>
                {
                    base.OnPaintBackground(e);

                    int _borderWidth = 2;
                    using (Pen p = new Pen(_headerbackcolor, _borderWidth))
                    {
                        Rectangle r = ClientRectangle;                        
                        r.Inflate(-Convert.ToInt32(_borderWidth / 2.0 + .5), -Convert.ToInt32(_borderWidth / 2.0 + .5));
                        e.Graphics.DrawRectangle(p, r);
                    }

                };
                this.Controls.Add(fp);                
                searchInput.Focus();
                searchInput.Clear();                

                DoubleBuffered = true;
            }

            private void DrawItemHandler(object sender, DrawItemEventArgs e)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    e.DrawBackground();                    

                    Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Brushes.YellowGreen : new SolidBrush(e.BackColor);

                    e.Graphics.FillRectangle(brush, e.Bounds);

                    //Font f = new System.Drawing.Font(e.Font.FontFamily, e.Font.Size, (((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? FontStyle.Bold : e.Font.Style));
                    
                    Font f = e.Font;

                    string MyString = listBox.GetItemText(listBox.Items[e.Index]);
                    string stringToFind = (!searchInput.Text.Equals(searchInput.Cue) ? searchInput.Text : string.Empty);

                    if (!string.IsNullOrEmpty(stringToFind))
                    {                       

                        string[] strings = MyString.Split(new string[] { stringToFind }, StringSplitOptions.None);

                        Rectangle rect = e.Bounds;

                        for (int i = 0; i < strings.Length; i++)
                        {
                            string s = strings[i];
                            if (!string.IsNullOrEmpty(s))
                            {
                                int width = (int)e.Graphics.MeasureString(s, f, e.Bounds.Width, StringFormat.GenericTypographic).Width;
                                rect.Width = width;
                                TextRenderer.DrawText(e.Graphics, s, f, new Point(rect.X+2, rect.Y), listBox.ForeColor);
                                rect.X += width+2;
                            }

                            if (i < strings.Length - 1)
                            {
                                int width2 = (int)e.Graphics.MeasureString(stringToFind, f, e.Bounds.Width, StringFormat.GenericTypographic).Width;
                                rect.Width = width2;
                                TextRenderer.DrawText(e.Graphics, stringToFind, f, new Point(rect.X+2 , rect.Y), listBox.ForeColor, Color.Yellow);
                                rect.X += width2+2;
                            }
                        }
                    }
                    else
                    {                        
                        TextRenderer.DrawText(e.Graphics, MyString, f, new Point(e.Bounds.X, e.Bounds.Y), listBox.ForeColor);
                    }

                    e.DrawFocusRectangle();

                });

            }
            
        }

        #endregion
        
        #region OpenFileBox
        public class OpenFileBox : TextBox
        {
            private BindingSource sourceCollection = new BindingSource();
            private string displaymember;
            private IComponentChangeService _changeService;
            private Image _isopenimg;
            private Image _openImage;
            private Size _ImageSize;
            private string cue = "Choose path to proceed";
            private bool openType = true;

            public override DockStyle Dock
            {
                get
                {
                    return base.Dock;
                }
                set
                {
                    base.Dock = value;
                }
            }

            public override AnchorStyles Anchor
            {
                get
                {
                    return base.Anchor;
                }
                set
                {
                    base.Anchor = value;
                }
            }

            public override System.ComponentModel.ISite Site
            {
                get
                {
                    return base.Site;
                }
                set
                {
                    _changeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
                    if (_changeService != null)
                        _changeService.ComponentChanged -= new ComponentChangedEventHandler(OnComponentChanged);
                    base.Site = value;
                    if (!DesignMode)
                        return;
                    _changeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
                    if (_changeService != null)
                        _changeService.ComponentChanged += new ComponentChangedEventHandler(OnComponentChanged);
                }
            }

            public BindingSource SearchCollection
            {
                get { return sourceCollection; }
                set { sourceCollection = value; }
            }

            public string SearchDisplayMember
            {
                get { return displaymember; }
                set { displaymember = value; }
            }

            [DisplayName("Open File or Directory")]
            [Description("Choose ToolBar return FileDialog or directory path")]
            [TypeConverter(typeof(openClassConverter))]
            public bool OpenType
            {
                get { return openType; }
                set { openType = value; }
            }

            protected Size ImageSize
            {
                get { return _ImageSize; }
            }

            public Image OpenImage
            {
                get { return _openImage; }
                set
                {
                    if (value == null)
                        _ImageSize = Size.Empty;
                    else
                        _ImageSize = value.Size;

                    _openImage = value;
                    this.Clear();
                    this.Text = this.cue;
                    this.Invalidate();
                }
            }

            public Image IsOpenImage
            {
                get { return _isopenimg; }
                set
                {
                    if (value == null)
                        _ImageSize = Size.Empty;
                    else
                        _ImageSize = value.Size;

                    _isopenimg = value;
                }
            }

            [Localizable(true)]
            public string Cue
            {
                get { return cue; }
                set
                {
                    cue = value;
                    this.Text = cue;
                    updateCue();
                }
            }

            private void OnComponentChanged(object sender, ComponentChangedEventArgs ce)
            {
                OpenFileBox sb = ce.Component as OpenFileBox;
                if (sb == null || !sb.DesignMode)
                    return;
                if (((IComponent)ce.Component).Site == null || ce.Member == null || ce.Member.Name != "Text")
                    return;
                if (sb.Text == sb.Name)
                    sb.Text = this.cue;
            }

            private void updateCue()
            {
                if (this.IsHandleCreated && cue != null)
                    SendMessage(this.Handle, 0x1501, (IntPtr)1, cue);
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                updateCue();
            }

            class CustomButton : System.Windows.Forms.Button
            {
                private bool _DisplayFocusCues = false;

                public CustomButton()
                    : base()
                {
                    this.SetStyle(ControlStyles.Selectable, false);
                }

                protected override bool ShowFocusCues
                {
                    get { return _DisplayFocusCues; }
                }

                public bool DisplayFocusCues
                {
                    get { return _DisplayFocusCues; }
                    set { _DisplayFocusCues = value; }
                }
            }

            public OpenFileBox()
                : base()
            {
                //SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                //SetStyle(ControlStyles.UserPaint, true);

                if (this.cue == null || string.IsNullOrEmpty(this.cue.ToString()))
                    this.cue = "Search";

                this.TextChanged += OpenFileBox_TextChanged;
                this.GotFocus += (s1, e1) => { if (!string.IsNullOrEmpty(this.Text) && this.Text.Equals(cue)) this.Text = string.Empty; };
                this.Text = this.cue;
                ForeColor = Color.FromArgb(20, 20, 20); // SystemColors.GrayText;
                DoubleBuffered = true;
            }

            public void OpenFileBox_TextChanged(object sender, EventArgs e)
            {
                if (sourceCollection != null && !string.IsNullOrEmpty(this.Text) && !this.Text.Equals(cue))
                {
                    sourceCollection.Filter = string.Format("[{0}] LIKE '%{1}%'",
                                displaymember,
                                this.Text);
                }
                else sourceCollection.Filter = null;

                if (this.Text.Equals(cue))
                    ForeColor = Color.FromArgb(20, 20, 20); // SystemColors.GrayText;
                else
                    ForeColor = SystemColors.WindowText;

                this.Controls.Clear();

                if (_openImage == null && _isopenimg == null)
                    return;
                CustomButton btn = new CustomButton();
                btn.AutoSize = true;
                btn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                btn.Dock = DockStyle.Right;
                btn.Cursor = Cursors.Default;
                if (!string.IsNullOrEmpty(this.Text) && !this.Text.Equals(this.cue.ToString()))
                    btn.Image = _isopenimg;
                else
                    btn.Image = _openImage;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Location = new Point(this.ClientSize.Width - btn.Width, -1);
                btn.Click += (s1, e1) =>
                {

                    this.Text = string.Empty;
                    if (openType)
                    {
                        OpenFileDialog fd = new OpenFileDialog();
                        fd.CheckFileExists = true;
                        fd.DefaultExt = "xml";
                        fd.Multiselect = true;

                        if (fd.ShowDialog(this) == DialogResult.OK)
                        {
                            this.Text = "\"" + string.Join("\";\"", fd.FileNames) + "\"";
                        }
                    }
                    else
                    {
                        FolderBrowserDialog fd = new FolderBrowserDialog();
                        fd.ShowNewFolderButton = true;

                        if (fd.ShowDialog(this) == DialogResult.OK)
                        {
                            this.Text = fd.SelectedPath;
                        }
                    }

                };
                this.Controls.Add(btn);
                // Send EM_SETMARGINS to prevent text from disappearing underneath the button
                SendMessage(this.Handle, 0xd3, (IntPtr)2, (IntPtr)(btn.Width << 16));

            }
                        
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, string lp);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        }

        public class openClassConverter : BooleanConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destType)
            {
                return (bool)value ?
                "File" : "Directory";
            }
            public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture,
            object value)
            {
                return (string)value == "File";
            }
        }

        #endregion
    }
}