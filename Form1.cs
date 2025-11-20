using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using XenoUI;

namespace RedSecuterA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            ClientsWindow.Initialize(false);
            InitializeComponent();
        }

        [DllImport("Xeno.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern IntPtr GetClients();

        [DllImport("Xeno.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern void Execute(byte[] script, int[] PIDs, int count);

        [DllImport("Xeno.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void Attach();

        private List<int> GetReadyClientPIDs()
        {
            var pids = new List<int>();

            try
            {
                IntPtr clientsPtr = GetClients();
                if (clientsPtr == IntPtr.Zero) return pids;

                string clientsJson = Marshal.PtrToStringAnsi(clientsPtr);
                var clientsList = JsonConvert.DeserializeObject<List<List<object>>>(clientsJson);

                if (clientsList != null)
                {
                    foreach (var client in clientsList)
                    {
                        if (client.Count >= 4)
                        {
                            int pid = Convert.ToInt32(client[0]);
                            int state = Convert.ToInt32(client[3]);

                            if (state == 3) // Ready state
                            {
                                pids.Add(pid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return pids;
        }

        private void ExecuteScriptOnClients(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                MessageBox.Show("Script is empty.", "Empty Script", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var clientPIDs = GetReadyClientPIDs();

            if (clientPIDs.Count == 0)
            {
                MessageBox.Show("No ready clients found.\n\nMake sure you've pressed Attach and waited for injection to complete.",
                    "No Ready Clients", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                Execute(Encoding.UTF8.GetBytes(script + "\0"), clientPIDs.ToArray(), clientPIDs.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Script execution failed:\n{ex.Message}", "Execution Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Attach();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string script = richTextBox1.Text;
            ExecuteScriptOnClients(script);
        }
    }
}
