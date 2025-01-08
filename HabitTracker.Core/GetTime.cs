using System.Net.Sockets;
using System.Net;

namespace HabitTracker.Core
{
  public static class GetTime
  {
    /// <summary>
    /// Получить актуальное время для конкретного пользователя.
    /// </summary>
    /// <param name="ntpServer"></param>
    /// <returns></returns>
    public static DateTime GetNetworkTime(string ntpServer)
    {
      const int ntpDataLength = 48;
      byte[] ntpData = new byte[ntpDataLength];
      ntpData[0] = 0x1B;

      IPEndPoint ipEndPoint = new IPEndPoint(Dns.GetHostAddresses(ntpServer)[0], 123);
      using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
      {
        socket.Connect(ipEndPoint);
        socket.Send(ntpData);
        socket.Receive(ntpData);
        socket.Close();
      }

      ulong intPart = BitConverter.ToUInt32(ntpData, 40);
      ulong fractPart = BitConverter.ToUInt32(ntpData, 44);

      intPart = SwapEndianness(intPart);
      fractPart = SwapEndianness(fractPart);

      ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
      DateTime networkDateTime = new DateTime(1900, 1, 1).AddMilliseconds((long)milliseconds);

      return networkDateTime.ToLocalTime();
    }

    /// <summary>
    /// Преобразует порядок байтов 32-битного числа, используя 64-битное представление.
    /// Метод принимает 64-битное число и возвращает 32-битное с измененным порядком байтов.
    /// </summary>
    /// <param name="x">64-битное число, для которого необходимо изменить порядок байтов.</param>
    /// <returns>32-битное число с переставленными байтами.</returns>
    private static uint SwapEndianness(ulong x)
    {
      return (uint)(((x & 0x000000FF) << 24) +
                    ((x & 0x0000FF00) << 8) +
                    ((x & 0x00FF0000) >> 8) +
                    ((x & 0xFF000000) >> 24));
    }
  }
}
