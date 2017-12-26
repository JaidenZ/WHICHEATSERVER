namespace WHICHEATSERVER.Core.Network
{
    using System;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;

    public static class SocketExtension
    {
        private static class NativeMethods
        {
            [DllImport("ws2_32.dll", SetLastError = true)]
            public static extern SocketError shutdown([In] IntPtr socketHandle, [In] SocketShutdown how);
        }

        public static void Close(Socket s)
        {
            if (s != null)
            {
                NativeMethods.shutdown(s.Handle, SocketShutdown.Both);
                s.Close();
                s.Dispose();
            }
        }
    }
}
