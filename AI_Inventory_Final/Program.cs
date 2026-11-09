using System;
using System.Windows.Forms;

namespace AI_Inventory_Final
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Login -> workspace -> (sign out) -> login again, until the user closes the window.
            while (true)
            {
                using (LoginForm login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK) break;
                }

                // Staff go straight to the cashier terminal; Admin / Manager get the full dashboard.
                Form workspace = LoginForm.LoggedInRole == "Staff" ? new Form10() : new Form1();
                Application.Run(workspace);

                if (!LoginForm.LogoutRequested) break; // window closed with X -> exit app
                LoginForm.LogoutRequested = false;
            }
        }
    }
}
