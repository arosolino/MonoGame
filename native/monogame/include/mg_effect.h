// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once
#include <string.h>

#if defined(_GAMING_XBOX)
#include <gxdk.resources.h>
#include <windows.h>
void MGG_EffectResource_GetBytecode(const char* name, mgbyte*& bytecode, mgint& size)
{
    bytecode = nullptr;
    size = 0;

    int id = 0;

    if (strcmp(name, "AlphaTestEffect") == 0)
        id = C_AlphaTestEffect;
    else if (strcmp(name, "BasicEffect") == 0)
        id = C_BasicEffect;
    else if (strcmp(name, "DualTextureEffect") == 0)
        id = C_DualTextureEffect;
    else if (strcmp(name, "EnvironmentMapEffect") == 0)
        id = C_EnvironmentMapEffect;
    else if (strcmp(name, "SkinnedEffect") == 0)
        id = C_SkinnedEffect;
    else if (strcmp(name, "SpriteEffect") == 0)
        id = C_SpriteEffect;

    if (id == 0)
        return;

    HMODULE module = nullptr;
    
    GetModuleHandleExW(
        GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS,
        (LPCWSTR)&MGG_EffectResource_GetBytecode,
        &module);

    // CAn't find resource
    HRSRC resource = FindResourceW(
        module,
        MAKEINTRESOURCEW(id),
        L"BIN");

    if (!resource)
        return;

    HGLOBAL loaded = LoadResource(module, resource);

    bytecode = static_cast<mgbyte*>(LockResource(loaded));
    size = static_cast<mgint>(SizeofResource(module, resource));
}
#else
#if defined(MG_DIRECTX12)
#define MG_BUILTIN_EFFECT_SYMBOL(name) name##_dx12_mgfxo
#elif defined(MG_VULKAN)
#define MG_BUILTIN_EFFECT_SYMBOL(name) name##_vk_mgfxo
#else
#error "Unsupported graphics backend, this header is intended for native builtin effects embedding only."
#endif
#define MG_BUILTIN_EFFECT_BYTES(name) ((mgbyte*)MG_BUILTIN_EFFECT_SYMBOL(name))
#define MG_BUILTIN_EFFECT_SIZE(name) (sizeof(MG_BUILTIN_EFFECT_SYMBOL(name)))

#define MG_HANDLE_BUILTIN_EFFECT(effectName, bytecode, size) \
    if (strcmp(name, #effectName) == 0) \
    { \
        (bytecode) = MG_BUILTIN_EFFECT_BYTES(effectName); \
        (size) = MG_BUILTIN_EFFECT_SIZE(effectName); \
    }

void MGG_EffectResource_GetBytecode(const char* name, mgbyte * &bytecode, mgint & size)
{
	MG_HANDLE_BUILTIN_EFFECT(AlphaTestEffect, bytecode, size)
	else MG_HANDLE_BUILTIN_EFFECT(BasicEffect, bytecode, size)
	else MG_HANDLE_BUILTIN_EFFECT(DualTextureEffect, bytecode, size)
	else MG_HANDLE_BUILTIN_EFFECT(EnvironmentMapEffect, bytecode, size)
	else MG_HANDLE_BUILTIN_EFFECT(SkinnedEffect, bytecode, size)
	else MG_HANDLE_BUILTIN_EFFECT(SpriteEffect, bytecode, size)
}
#endif
