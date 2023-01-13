using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace test_upload_photo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string conString = @"Data Source=USER-PC\SQLEXPRESS;Initial Catalog=TestUploadPhoto;Integrated Security=True";

        //button load photo
        private void button1_Click(object sender, EventArgs e)
        {
            var uploader = new ImageUploader(conString);
            uploader.Upload(pictureBox1, 1);
        }
        class ImageUploader
        {
            private readonly string _connectionString;

            public ImageUploader(string connectionString)
            {
                _connectionString = connectionString;
            }

            public void Upload(PictureBox pictureBox, int id)
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO myTable (Id, Image) VALUES (@Id, @Image)";
                    command.Parameters.AddWithValue("@Id", id);

                    var image = new Bitmap(pictureBox.Image);
                    using (var memoryStream = new MemoryStream())
                    {
                        image.Save(memoryStream, ImageFormat.Jpeg);
                        memoryStream.Position = 0;

                        var sqlParameter = new SqlParameter("@Image", SqlDbType.VarBinary, (int)memoryStream.Length)
                        {
                            Value = memoryStream.ToArray()
                        };
                        command.Parameters.Add(sqlParameter);
                    }

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        class ImageRetriever
        {
            private readonly string _connectionString;

            public ImageRetriever(string connectionString)
            {
                _connectionString = connectionString;
            }

            public void Retrieve(PictureBox pictureBox, int id)
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Image FROM myTable WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var imageData = (byte[])reader["Image"];
                            using (var memoryStream = new MemoryStream(imageData))
                            {
                                pictureBox.Image = Image.FromStream(memoryStream);
                            }
                        }
                    }
                }
            }
        }
        //button select
        private void button3_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox2.Image = Image.FromFile(openFileDialog.FileName);
                }
            }
        }

        //button view photo
        private void button2_Click(object sender, EventArgs e)
        {
            var retriever = new ImageRetriever(conString);
            retriever.Retrieve(pictureBox2, 1);
        }
    }
}
