using System.Linq.Expressions;
using System.Text;

namespace encoder_decoder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            richTextBox1.MaxLength = 256; //setting cap on message length
        }
        //declaring variables
        string path = "";
        int newImageWidth;
        int newImageHeight;


        Bitmap displayPPM;//creating bitmaps
        Bitmap newImage;
        private void button1_Click(object sender, EventArgs e)
        {
            //declaring variables
            string message = richTextBox1.Text.ToUpper();
            Color byteColor = new Color();

            //creating altered image using FromBitmap function defined below
            pictureBox2.Image = FromBitmap();

            Bitmap FromBitmap()
            {
                //copying image dimensions
                pictureBox2.Width = displayPPM.Width;
                pictureBox2.Height = displayPPM.Height;

                //creating new bitmap
                newImage = new Bitmap(displayPPM.Width, displayPPM.Height);

                //declaring variables
                byte r;
                byte g;
                byte b;
                int i;
                int j;
                int k = 0; //counter var


                /*nested loop will iterate through the original image and gather pixel data. the data will be broken into color channel data
                 * blue channel data will be checked against parameters to ensure values fall outside of range used for encoding the message.
                 * within the selected range for message data blue values will be changed to corrrelate with ascii value for the chars of the message*/
                for (i = 0; i < displayPPM.Height; i++)
                {
                    for (j = 0; j < displayPPM.Width; j++)
                    {
                        byteColor = displayPPM.GetPixel(i, j);
                        r = byteColor.R;
                        g = byteColor.G;
                        b = byteColor.B;

                        if (i >= 1 && j >= 1)
                        {
                            if (k < message.Length)
                            {
                                b = (byte)Convert.ToInt32(message[k]); //getting ascii value for  message char 
                                k++;
                            }
                        }
                        else //alter blue chanel data to fall outside message range 
                        {
                            if (b > 43 && b < 65)
                            {
                                b = 43;

                            }
                            else if (byteColor.B >= 65 && byteColor.B < 91)
                            {
                                b = 91;
                            }
                            else if (byteColor.B == 32) //disallows data to be whitespace char code
                            {
                                b = 33;
                            }
                        }
                        //writes the new pixel to the image
                        byteColor = Color.FromArgb(r, g, b);
                        newImage.SetPixel(i, j, byteColor);
                    }
                }
                //display
                return newImage;
            }

        }


        public void openFile()
        {
            openFileDialog1.Filter = "Picture Files |  *.ppm"; //limits to ppm files

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                {
                    path = openFileDialog1.FileName; //path will be the name found in openFile dialog

                }
                try
                {
                    pictureBox1.Image = FromPPM(); //runs FromPPM function defined below

                    Bitmap FromPPM()
                    {
                        StreamReader sr = new StreamReader(path);

                        //reading header info
                        string format = sr.ReadLine(); //PPM3 or PPM6
                        string comment = sr.ReadLine();
                        string widthHeight = sr.ReadLine(); //PPM dimensions
                        string maxCV = sr.ReadLine(); //CV = color value

                        //storing dimension data into an array to split into separate ints
                        string[] dimArray = widthHeight.Split();
                        int width = int.Parse(dimArray[0]);
                        int height = int.Parse(dimArray[1]);
                        //will be used for dimensions of altered image
                        pictureBox1.Height = height;
                        pictureBox1.Width = width;

                        //assigning bitmap dimensions
                        displayPPM = new Bitmap(width, height);

                        //declaring vars to store PPM RGB data
                        int red;
                        int green;
                        int blue;

                        if (format.Contains('3'))
                        {

                            //looping thru pixels
                            for (int y = 0; y < height; y++)
                            {
                                for (int x = 0; x < width; x++)
                                {
                                    //collect ppm elements
                                    red = int.Parse(sr.ReadLine());
                                    green = int.Parse(sr.ReadLine());
                                    blue = int.Parse(sr.ReadLine());

                                    //convert RGB values to color
                                    Color pixelColor = Color.FromArgb(red, green, blue);

                                    //draw pixel
                                    displayPPM.SetPixel(x, y, pixelColor);
                                }
                            }
                        }
                        else if (format.Contains('6'))
                        {

                            int x = 0;
                            int y = 0;

                            using (FileStream fs = File.OpenRead(path))
                            {
                                for (int i = 0; i < 4;)
                                {
                                    if (fs.ReadByte() == (int)'\n')
                                    {
                                        i++;
                                    }
                                }


                                for (y = 0; y < height; y++)
                                {
                                    for (x = 0; x < width; x++)
                                    {
                                        red = fs.ReadByte();
                                        green = fs.ReadByte();
                                        blue = fs.ReadByte();

                                        //convert RGB values to color
                                        Color pixelColor = Color.FromArgb(red, green, blue);

                                        //draw pixel
                                        displayPPM.SetPixel(x, y, pixelColor);
                                    }
                                }

                            }
                        }

                        //creating limit on richTextBox1 message length
                        int maxMessage = (width - 1) * (height - 1);
                        richTextBox1.MaxLength = maxMessage;
                        //return the bitmap
                        return displayPPM;
                    }
                }
                catch (Exception error)
                {
                    // Let the user know what went wrong.
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(error.Message);
                }
            }
        }
        //function to be called when user clicks the open button in the toolstrip
        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            openFile();//runs openFile function defined above
        }

        //function to be called when user clicks the save button in the toolstrip
        private void saveFile()
        {
            string filePath;
            saveFileDialog1.Filter = "Picture Files |  *.ppm"; //limits to ppm files

            if (saveFileDialog1.ShowDialog() == DialogResult.OK) //when ok is clicked by user the saved file will have the name entered into save file dialog
            {
                {
                    filePath = saveFileDialog1.FileName;

                }
                try
                {
                    using (StreamWriter sw = new StreamWriter(filePath))
                    {
                        //declaring variables to store image data
                        string width = newImage.Width.ToString();
                        string height = newImage.Height.ToString();

                        //writing file header
                        sw.WriteLine("P3");
                        sw.WriteLine("#");
                        sw.WriteLine(width + " " + height);
                        sw.WriteLine("255");

                        //nested loop will iterate through the image and write pixel info to the file
                        for (int i = 0; i < newImage.Height; i++)
                        {
                            for (int j = 0; j < newImage.Width; j++)
                            {
                                Color pixelColor = newImage.GetPixel(j, i);

                                sw.WriteLine(pixelColor.R);
                                sw.WriteLine(pixelColor.G);
                                sw.WriteLine(pixelColor.B);
                            }
                        }
                        sw.Close();
                    }
                    richTextBox1.Text = "Saved"; //output to user
                }
                catch (Exception error)
                {
                    // Let the user know what went wrong.
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(error.Message);
                }
            }
        }
        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            saveFile();//will run saveFile function defined above
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        //displays a how to
        {
            MessageBox.Show("How To Use: \nClick the Open File Icon to upload a PPM file and enter message to be encoded in text box. Click 'Encode' and the message will be encoded into the image.\n" +
                "Click the Save File icon to save the encoded image to a new PPM file.\n" +
                "TIPS:\n" +
                "The program works by altering the blue color channel for selected pixels. Try and use an image that is visually busy and does not have large patches of a single color.\n" +
                "Try to avoid 'flat' images.\n" +
                "Dark images and large images work best.");
        }
    }
}