// Inserted after the stock search. All scratch registers are volatile on Win64.
// The loader fills in the image base (ASLR) before creating each hook.
.intel_syntax noprefix
.text
#if defined(DISPLAY)
    test eax, eax
    jnz done
#define ARCANA r15b
#elif defined(POINTS)
#define ARCANA sil
#else
#define ARCANA dil
#endif
    movabs r11, 0x123456789abcdef0
    mov r9, [r11 + 0x11a7038]  // 256 saved entries, 48 bytes each
    mov r10, [r11 + 0xec0958]  // persona metadata, 14 bytes each
    test r9, r9
    jz done
    test r10, r10
    jz done
    test ARCANA, ARCANA
    jz done
    mov ecx, 1                 // zero is the game's no-persona result
next:
    add r9, 48
    add r10, 14
    test byte ptr [r9], 1
    jz advance
    test byte ptr [r10], 0x28  // exclusions in the compendium list builder
    jnz advance
    cmp byte ptr [r10 + 2], ARCANA
    jne advance
#ifdef DISPLAY
    mov eax, ecx               // a registered name for the dialogue
#else
    mov r8, [r11 + 0x11a6b80]
    movss xmm6, [r8 + 8]       // the game's configured matching bonus
#endif
    jmp done
advance:
    inc ecx
    cmp ecx, 256
    jb next
done:
    nop
