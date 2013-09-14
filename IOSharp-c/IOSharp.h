#ifndef IOSHARP_H_INCLUDED
#define IOSHARP_H_INCLUDED
#include <stdint.h>

typedef struct spi_config
{
  int mode;
  uint32_t speed;
  int cs_change;
  uint16_t delay;
} SPI_CONFIG;

uint64_t start_polling(int pin);
void InternalWriteReadByte(unsigned char writeBuffer[], int writeOffset, int writeCount, unsigned char readBuffer[], int readOffset, int readCount, int startReadOffset, SPI_CONFIG spi);
void InternalWriteReadShort(unsigned short writeBuffer[], int writeOffset, int writeCount, unsigned short readBuffer[], int readOffset, int readCount, int startReadOffset, SPI_CONFIG spi);
#endif