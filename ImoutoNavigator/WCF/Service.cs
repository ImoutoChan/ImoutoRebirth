using System;
using System.ServiceModel;
using Imouto.WCFExchageLibrary.Operations;

namespace Imouto.Navigator.WCF
{
    delegate TResult UseServiceDelegate<TResult>(IImoutoWCFService proxy);
    delegate void UseServiceDelegate(IImoutoWCFService proxy);

    static class ImoutoService
    {
        public static ChannelFactory<IImoutoWCFService> _channelFactory
            = new ChannelFactory<IImoutoWCFService>(
                new NetNamedPipeBinding(),
                new EndpointAddress("net.pipe://localhost/ImoutoServiceWcf"));

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
