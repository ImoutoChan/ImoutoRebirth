using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Imouto.WcfExchangeLibrary.Core.Operations;

namespace ImoutoRebirth.Navigator.WCF
{
    delegate TResult UseCollectionServiceDelegate<out TResult>(IImoutoWCFCollectionService proxy);
    delegate void UseCollectionServiceDelegate(IImoutoWCFCollectionService proxy);

    static class ImoutoCollectionService
    {
        public static ChannelFactory<IImoutoWCFCollectionService> _channelFactory
            = new ChannelFactory<IImoutoWCFCollectionService>(
                new CustomBinding((Binding)null),
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
