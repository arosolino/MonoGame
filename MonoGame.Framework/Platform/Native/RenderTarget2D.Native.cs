// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
using static System.Net.Mime.MediaTypeNames;
namespace Microsoft.Xna.Framework.Graphics;

public partial class RenderTarget2D
{
    private unsafe void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
    {
        Handle = MGG.RenderTarget_Create(
            GraphicsDevice.Handle,
            TextureType._2D,
            _format,
            width,
            height,
            1,
            _levelCount,
            ArraySize,
            preferredDepthFormat,
            preferredMultiSampleCount,
            usage);
    }

    private unsafe void PlatformGraphicsDeviceResetting()
    {
        /*
        if (Handle != null && Owned)
        {
            MGG.Texture_Destroy(GraphicsDevice.Handle, Handle);
            Handle = null;

            Handle = MGG.RenderTarget_Create(
                GraphicsDevice.Handle,
                TextureType._2D,
                _format,
                width,
                height,
                1,
                _levelCount,
                ArraySize,
                DepthStencilFormat,
                MultiSampleCount,
                RenderTargetUsage);
        }
        */
    }

/*
 * 	    public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared, int arraySize)
	        : base(graphicsDevice, width, height, mipMap, QuerySelectedFormat(graphicsDevice, preferredFormat), SurfaceType.RenderTarget, shared, arraySize)
	    {
            DepthStencilFormat = preferredDepthFormat;
            MultiSampleCount = graphicsDevice.GetClampedMultisampleCount(preferredMultiSampleCount);
            RenderTargetUsage = usage;

            PlatformConstruct(graphicsDevice, width, height, mipMap, preferredDepthFormat, preferredMultiSampleCount, usage, shared);
	    }*/
}
