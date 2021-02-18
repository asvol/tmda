using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Asv.Tmda.Core
{
    public abstract class ThreadSafeAnalyzerIqBase : IAnalyzerIq
    {
        private readonly TaskFactory _taskFactory;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly SingleThreadTaskScheduler _ts;
        private int _isDisposed;
        private bool _isOpened;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        protected CancellationToken DisposeCancel => _cancel.Token;
        protected TaskFactory TaskFactory => _taskFactory;

        protected ThreadSafeAnalyzerIqBase()
        {
            _taskFactory = new TaskFactory(_ts = new SingleThreadTaskScheduler("SH"));
        }

        public async Task Open(CancellationToken cancel)
        {
            var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel, DisposeCancel);
            try
            {
                if (_isOpened == true) await TaskFactory.StartNew(InternalClose, linkedCancel.Token);
                await TaskFactory.StartNew(InternalOpen, linkedCancel.Token);
                _isOpened = true;
            }
            catch (Exception e)
            {
                _logger.Error($"Error to open device {this}: {e.Message}");
                throw;
            }
            finally
            {
                linkedCancel.Dispose();
            }
            
        }

        public bool IsOpened => _isOpened;
     
        protected abstract void InternalOpen();

        

        public async Task Close(CancellationToken cancel)
        {
            if (_isOpened == false) return;
            var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel, DisposeCancel);
            try
            {
                await TaskFactory.StartNew(()=>
                {
                    if (_isOpened == false) return;
                    InternalClose();
                }, linkedCancel.Token);
                _isOpened = false;
            }
            catch (Exception e)
            {
                _logger.Error($"Error to close device {this}: {e.Message}");
                throw;
            }
            finally
            {
                linkedCancel.Dispose();
            }
            
        }

        protected abstract void InternalClose();

        protected void InternalCheckOpened()
        {
            if (!_isOpened) throw new Exception("Device not opened");
        }

        public async Task<AnalyzerIqLimits> GetLimits(CancellationToken cancel)
        {
            var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel, DisposeCancel);
            try
            {
                return await TaskFactory.StartNew(() =>
                {
                    InternalCheckOpened();
                    return InternalGetLimits();
                }, linkedCancel.Token);
            }
            catch (Exception e)
            {
                _logger.Error($"Error to close device {this}: {e.Message}");
                throw;
            }
            finally
            {
                linkedCancel.Dispose();
            }
        }

        protected abstract AnalyzerIqLimits InternalGetLimits();

        public async Task SetConfig(AnalyzerIqConfig cfg, CancellationToken cancel)
        {
            var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel, DisposeCancel);
            try
            {
                await TaskFactory.StartNew(()=>
                {
                    InternalCheckOpened();
                    InternalSetConfig(cfg);
                }, linkedCancel.Token);
            }
            catch (Exception e)
            {
                _logger.Error($"Error to set config {this}: {e.Message}");
                throw;
            }
            finally
            {
                linkedCancel.Dispose();
            }
        }

        public async Task SetFreq(double freqHz, CancellationToken cancel)
        {
            var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel, DisposeCancel);
            try
            {
                await TaskFactory.StartNew(() =>
                {
                    InternalCheckOpened();
                    InternalSetFreq(freqHz);
                }, linkedCancel.Token);
            }
            catch (Exception e)
            {
                _logger.Error($"Error to get config {this}: {e.Message}");
                throw;
            }
            finally
            {
                linkedCancel.Dispose();
            }
        }

        protected abstract void InternalSetFreq(double freqHz);

        protected abstract void InternalSetConfig(AnalyzerIqConfig cfg);

        public async Task<AnalyzerIqInfo> GetConfig(CancellationToken cancel)
        {
            var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel, DisposeCancel);
            try
            {
                return await TaskFactory.StartNew(()=>
                {
                    InternalCheckOpened();
                    return InternalGetConfig();
                }, linkedCancel.Token);
            }
            catch (Exception e)
            {
                _logger.Error($"Error to get config {this}: {e.Message}");
                throw;
            }
            finally
            {
                linkedCancel.Dispose();
            }
        }

        protected abstract AnalyzerIqInfo InternalGetConfig();

        public async Task<AnalyzerIqPacket> Read(AnalyzerIqRequest query, CancellationToken cancel)
        {
            var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel, DisposeCancel);
            try
            {
                return await TaskFactory.StartNew(()=>
                {
                    InternalCheckOpened();
                    try
                    {
                        return InternalRead(query);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    
                }, linkedCancel.Token);
            }
            catch (Exception e)
            {
                _logger.Error($"Error to read IQ from device {this}: {e.Message}");
                throw;
            }
            finally
            {
                linkedCancel.Dispose();
            }
        }

        protected abstract AnalyzerIqPacket InternalRead(AnalyzerIqRequest query);

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed,1,0) != 0) return;
            Close(CancellationToken.None).Wait(DisposeCancel);
            TaskFactory.StartNew(InternalDisposeOnce, DisposeCancel).Wait(DisposeCancel);
            _cancel.Cancel(false);
            _cancel.Dispose();
            _ts.Dispose();
        }

        protected abstract void InternalDisposeOnce();
    }
}