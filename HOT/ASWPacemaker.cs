using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Valve.VR;


namespace OculusHack
{
    class ASWPacemaker
    {
        public float GPUthreshold { get; set; }
        public float CPUthreshold { get; set; }

        public static void InitOpenVR()
        {
            var error = EVRInitError.None;

            OpenVR.Init(ref error, EVRApplicationType.VRApplication_Background);
            if (error != EVRInitError.None) throw new Exception();

            //OpenVR.GetGenericInterface(OpenVR.IVRCompositor_Version, ref error);
            //if (error != EVRInitError.None) throw new Exception();

            //OpenVR.GetGenericInterface(OpenVR.IVROverlay_Version, ref error);
            //if (error != EVRInitError.None) throw new Exception();
            
        }

        private async void StartPacemaker()
        {
            Compositor_FrameTiming cft = new Compositor_FrameTiming();
            cft.m_nSize = (uint)Marshal.SizeOf(typeof(Compositor_FrameTiming));

            await Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(100); // This sucks but will do for now
                    OpenVR.Compositor.GetFrameTiming(ref cft, 0);
                    //Console.WriteLine(cft.m_flTotalRenderGpuMs);

                }
            });
        }


    }
}
