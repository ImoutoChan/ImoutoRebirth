using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Imouto.WcfExchangeLibrary.Core.Operations;

namespace ImoutoRebirth.Navigator.WCF
{
    delegate TResult UseServiceDelegate<TResult>(IImoutoWCFService proxy);
    delegate void UseServiceDelegate(IImoutoWCFService proxy);

    static class ImoutoService
    {
        public static ChannelFactory<IImoutoWCFService> _channelFactory
            = new ChannelFactory<IImoutoWCFService>(
                new CustomBinding((Binding)null),
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

        public static async Task<TResult> UseAsync<TResult>(UseServiceDelegate<TResult> codeBlock)
        {
            return await Task.Run(() =>
            {
                IClientChannel proxy = (IClientChannel) _channelFactory.CreateChannel();
                bool success = false;
                bool getResultSuccess = false;
                TResult result = default(TResult);

                try
                {

                    result = codeBlock((IImoutoWCFService) proxy);
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
                    catch
                    {
                    }

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
            });
        }

        public static async Task UseAsync(UseServiceDelegate codeBlock)
        {
            await Task.Run(() =>
            {
                IClientChannel proxy = (IClientChannel) _channelFactory.CreateChannel();
                bool success = false;
                bool getResultSuccess = false;

                try
                {
                    codeBlock((IImoutoWCFService) proxy);
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
                    catch
                    {
                    }

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
            });
        }
    }
}
