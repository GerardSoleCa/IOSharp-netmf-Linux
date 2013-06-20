#ifndef IOSHARP_H_INCLUDED
#define IOSHARP_H_INCLUDED
#include <stdint.h>
uint64_t start_polling(int pin);

/* SPI */
void InternalWriteRead(uint16_t[] writeBuffer, int writeOffset, int writeCount, uint16_t[] readBuffer, int readOffset, int readCount, int startReadOffset);
void InternalWriteRead(uint8_t[] writeBuffer, int writeOffset, int writeCount, uint8_t[] readBuffer, int readOffset, int readCount, int startReadOffset);
#endif