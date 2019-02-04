using System.Windows.Forms;
using System;
using System.Drawing;

namespace StateMigration.UI
{
    public partial class mainFrom
    {
        #region ContextMenu

        private void addItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!mtbtnCancel.Enabled)
            {
                fbdDefaults.Description = "Select Item";
                fbdDefaults.RootFolder = Environment.SpecialFolder.MyComputer;
                if (fbdDefaults.ShowDialog() == DialogResult.OK)
                {
                    lstViewDefaultPaths.Items.Add(fbdDefaults.SelectedPath);
                    lstViewDefaultPaths.Invalidate();
                    regions.SetPath(fbdDefaults.SelectedPath);
                    custompaths.Add(fbdDefaults.SelectedPath);
                    //SettingResponseLabelInfo($"Selected Source : {objPerson.CustomHomeDirectory}");

                }

            }            

            
        }

        private void removeItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if(!mtbtnCancel.Enabled)
            {
                if (lstViewDefaultPaths.SelectedIndex != -1)
                {
                    for (int i = lstViewDefaultPaths.SelectedItems.Count - 1; i >= 0; i--)
                        lstViewDefaultPaths.Items.Remove(lstViewDefaultPaths.SelectedItems[i]);
                }
            }
            


        }

        #endregion

        #region MouseGestures

        private void lstViewDefaultPaths_MouseEnter(object sender, EventArgs e)
        {
            mtdefaultReslbl.UseCustomBackColor = true;
            mtdefaultReslbl.UseCustomForeColor = true;
            mtdefaultReslbl.ForeColor = Color.White;
            mtdefaultReslbl.BackColor = Color.FromArgb(72, 134, 21);
            mtdefaultReslbl.Text = "Right click anywhere to add or remove items.";

            if(mtbtnCancel.Enabled)
            {
                mtdefaultReslbl.Text = "An Operation is in progress.";
            }
        }

        private void lstViewDefaultPaths_MouseLeave(object sender, EventArgs e)
        {

            mtdefaultReslbl.UseCustomBackColor = true;
            mtdefaultReslbl.UseCustomForeColor = true;
            mtdefaultReslbl.ForeColor = Color.White;
            mtdefaultReslbl.BackColor = Color.FromArgb(211, 159, 23);
            mtdefaultReslbl.Text = "Pre-Defined paths for Backup && Restore operations.";
        }

        #endregion

    }
}

