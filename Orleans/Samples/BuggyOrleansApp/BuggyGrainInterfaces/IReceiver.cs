using System.Threading.Tasks;
using Orleans;

namespace BuggyOrleansApp
{
    /// <summary>
    /// Grain interface IClient
    /// </summary>
	public interface IReceiver : IGrainWithIntegerKey
    {
        Task StartTransaction();

        Task TransmitData(string item);

        Task<int> GetCurrentCount();
    }
}
