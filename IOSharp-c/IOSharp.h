#ifndef IOSHARP_H_INCLUDED
#define IOSHARP_H_INCLUDED
#include <stdint.h>
uint64_t start_polling(int pin);
void InternalWriteRead(unsigned char writeBuffer[], int writeOffset, int writeCount, unsigned char readBuffer[], int readOffset, int readCount, int startReadOffset, uint32_t speed);
#endif