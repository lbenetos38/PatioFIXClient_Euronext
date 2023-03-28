using PatioFIX.Common.DAL;
using System.Diagnostics;

namespace PatioFIX.Common.BLL
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseManager
    {

        /// <summary>
        /// 
        /// </summary>
        protected BaseManager()
        {

        }


        #region Dals

        IOdlDataLayer m_odlDataLayer = null;
        internal IOdlDataLayer OdlDal
        {
            [DebuggerStepThrough]
            get
            {
                if (m_odlDataLayer == null)
                {
                    if (Globals.PatioOMS.DisableDataLayer == false)
                        m_odlDataLayer = new OdlDataLayer();
                    else
                        m_odlDataLayer = new OdlDummyDatalayer();
                }
                return m_odlDataLayer;
            }
        }


        #endregion



    }
}
