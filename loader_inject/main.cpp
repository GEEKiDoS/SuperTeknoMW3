#include <ws2tcpip.h>
#include <windows.h>
#include <strsafe.h>
#include <stdint.h>

BOOL __stdcall DllMain(HMODULE hModule, DWORD dwReason, LPVOID lpReserved)
{
	if (dwReason == DLL_PROCESS_ATTACH)
	{
		*(DWORD*)0x4A36E1 = (DWORD)"^5SuperTeknoMW3 1.1.3 \n^7by A2ON.";
	}
	
	return true;
}