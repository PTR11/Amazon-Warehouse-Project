using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Brute_Force.ViewModel
{
    /// <summary>
    /// Class for ViewModel's table grid.
    /// </summary>
    public class Field : ViewModelBase
    {
        #region Fields

        private String _background;
        private String _image;
        private String _baseImage;
        private String _designation;
        private Int32 _width;
        private Int32 _heigth;
        private String _text;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public Field()
        {

        }
        /// <summary>
        /// Fully constructor to set up every elements.
        /// </summary>
        /// <param name="bg">Background</param>
        /// <param name="image">Image</param>
        /// <param name="baseimg">base Image</param>
        /// <param name="designation"></param>
        /// <param name="w">Width</param>
        /// <param name="h">Heigth</param>
        /// <param name="text">Text</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="number">Number</param>
        /// <param name="oc">OnClick</param>
        /// <param name="ds">DesignationShelf</param>
        /// <param name="orc">OnRightClick</param>
        public Field(String bg, String image, String baseimg, String designation, Int32 w, Int32 h, String text, Int32 x, Int32 y, Int32 number, DelegateCommand oc, DelegateCommand ds, DelegateCommand orc )
        {
            this.Background = bg;
            this.Image = image;
            this.BaseImage = baseimg;
            this.Designation = designation;
            this.Width = w;
            this.Height = h;
            this.Text = text;
            this.X = x;
            this.Y = y;
            this.Number = number;
            this.OnClick = oc;
            this.DesignationShelf = ds;
            this.OnRightClick = orc;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Query or set a caption.
        /// </summary>
        public String Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Query or set a background color.
        /// </summary>
        public String Background
        {
            get { return _background; }
            set
            {
                if (_background != value)
                {
                    _background = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Query or set a designation.
        /// </summary>
        public String Designation
        {
            get { return _designation; }
            set
            {
                if (_designation != value)
                {
                    _designation = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Query or set a default image.
        /// </summary>
        public String BaseImage
        {
            get { return _baseImage; }
            set
            {
                if (_baseImage != value)
                {
                    _baseImage = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Query or set a image.
        /// </summary>
        public String Image
        {
            get { return _image; }
            set
            {
                if (_image != value)
                {
                    _image = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Query or set the window width.
        /// </summary>
        public Int32 Width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Query or set a widnow height.
        /// </summary>
        public Int32 Height
        {
            get { return _heigth; }
            set
            {
                if (_heigth != value)
                {
                    _heigth = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Query or set X coordinate.
        /// </summary>
        public Int32 X { get; set; }
        /// <summary>
        /// Query or set Y coordinate.
        /// </summary>
        public Int32 Y { get; set; }
        /// <summary>
        /// Query or set index.
        /// </summary>
        public Int32 Number { get; set; }
        public DelegateCommand OnClick { get; set; }
        public DelegateCommand DesignationShelf { get;  set; }
        public DelegateCommand OnRightClick { get; set; }
        /// <summary>
        /// Create a copy of the object
        /// </summary>
        /// <returns>Returns az exact copy of that object</returns>
        public Field Copy()
        {
            return new Field(Background, Image, BaseImage, Designation, Width, Height, Text, X, Y, Number,OnClick,DesignationShelf,OnRightClick);
        }
        /// <summary>
        /// Clear main parts of the object
        /// </summary>
        public void Clear()
        {
            this.Designation = "Black";
            this.Background = "White";
            this.Text = "";
            this.BaseImage = "/images/empty.png";
            this.Image = "/images/empty.png";
        }
        #endregion
    }
}
