using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace sharpcommander
{
    class FileAcces
    {
        string[] trails;


        public FileAcces(string path)
        {
            try
            {
                try
                {
                    trails = Directory.GetFileSystemEntries(path);
                }
                catch (UnauthorizedAccessException uex)
                {
                    System.Windows.Forms.MessageBox.Show(uex.Message.ToString());
                }
            }
            catch (IOException iex)
            {
                System.Windows.Forms.MessageBox.Show(iex.Message.ToString());
            }
        }




        public string[] backpath()
        {
            return trails;
        }

        public static string backparent(string path)
        {
            try
            {
                return Directory.GetParent(path).ToString();
            }
            catch (NullReferenceException)
            {
                return path;
            }
        }

    }
}
