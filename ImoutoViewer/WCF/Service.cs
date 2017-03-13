using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Imouto.WCFExchageLibrary.Operations;

namespace Imouto.Viewer.WCF
{
    delegate TResult UseServiceDelegate<TResult>(IImoutoWCFService proxy);
    delegate void UseServiceDelegate(IImoutoWCFService proxy);

    static class ImoutoService
    {
        public static ChannelFactory<IImoutoWCFService> _channelFactory
            = new ChannelFactory<IImoutoWCFService>(
                new NetNamedPipeBinding
                {
                    MaxReceivedMessageSize = Int32.MaxValue,
                    MaxBufferSize = Int32.MaxValue
                },
                new EndpointAddress("net.pipe://localhost/ImoutoServiceWcf"));


        public static async Task<TResult> UseAsync<TResult>(UseServiceDelegate<TResult> codeBlock)
        {
            return await Task.Run(() => Use(codeBlock));
        }

        public static TResult Use<TResult>(UseServiceDelegate<TResult> codeBlock)
        {
            IClientChannel proxy = (IClientChannel)_channelFactory.CreateChannel();
            bool success = false;
            bool getResultSuccess = false;
            TResult result = default(TResult);

            try
            {

                result = codeBlock((IImoutoWCFService)proxy);
                getResultSuccess = true;

                proxy.Close();
                success = true;
            }
            catch (Exception ex)
            {
                try
                {
                    proxy.Close();
                    success = true;
                }
                catch { }

                if (!getResultSuccess)
                {
                    throw ex;
                }
            }
            finally
            {
                if (!success)
                {
                    proxy.Abort();
                }
            }

            return result;
        }

        public static async Task UseAsync(UseServiceDelegate codeBlock)
        {
            await Task.Run(() => Use(codeBlock));
        }

        public static void Use(UseServiceDelegate codeBlock)
        {
            IClientChannel proxy = (IClientChannel)_channelFactory.CreateChannel();
            bool success = false;
            bool getResultSuccess = false;

            try
            {
                codeBlock((IImoutoWCFService)proxy);
                getResultSuccess = true;

                proxy.Close();
                success = true;
            }
            catch (Exception ex)
            {
                try
                {
                    proxy.Close();
                    success = true;
                }
                catch { }

                if (!getResultSuccess)
                {
                    throw ex;
                }
            }
            finally
            {
                if (!success)
                {
                    proxy.Abort();
                }
            }
        }
    }
}
