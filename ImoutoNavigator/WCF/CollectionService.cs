using System;
using System.ServiceModel;
using WCFExchageLibrary.Operations;

namespace ImoutoNavigator.WCF
{
    delegate TResult UseCollectionServiceDelegate<out TResult>(IImoutoWCFCollectionService proxy);
    delegate void UseCollectionServiceDelegate(IImoutoWCFCollectionService proxy);

    static class ImoutoCollectionService
    {
        public static ChannelFactory<IImoutoWCFCollectionService> _channelFactory
            = new ChannelFactory<IImoutoWCFCollectionService>(
                new NetNamedPipeBinding(),
                new EndpointAddress("net.pipe://localhost/ImoutoServiceWcfCollection"));

        public static TResult Use<TResult>(UseCollectionServiceDelegate<TResult> codeBlock)
        {
            IClientChannel proxy = (IClientChannel)_channelFactory.CreateChannel();
            bool success = false;
            bool getResultSuccess = false;
            TResult result = default(TResult);

            try
            {

                result = codeBlock((IImoutoWCFCollectionService)proxy);
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

        public static void Use(UseCollectionServiceDelegate codeBlock)
        {
            IClientChannel proxy = (IClientChannel)_channelFactory.CreateChannel();
            bool success = false;
            bool getResultSuccess = false;

            try
            {
                codeBlock((IImoutoWCFCollectionService)proxy);
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
