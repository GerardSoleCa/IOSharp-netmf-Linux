#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <unistd.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>
#include <poll.h>

/* SPI */
#include <getopt.h>
#include <fcntl.h>
#include <sys/ioctl.h>
#include <linux/types.h>
#include <linux/spi/spidev.h>

/*

   ____ ____ ___ ___    _____ _   _ _   _  ____ _____ ___ ___  _   _ ____  
  / ___|  _ |_ _/ _ \  |  ___| | | | \ | |/ ___|_   _|_ _/ _ \| \ | / ___| 
 | |  _| |_) | | | | | | |_  | | | |  \| | |     | |  | | | | |  \| \___ \ 
 | |_| |  __/| | |_| | |  _| | |_| | |\  | |___  | |  | | |_| | |\  |___) |
  \____|_|  |___\___/  |_|    \___/|_| \_|\____| |_| |___\___/|_| \_|____/
                                                                     

*/

 /****************************************************************
 * Constants
 ****************************************************************/
 
#define SYSFS_GPIO_DIR "/sys/class/gpio"
#define POLL_TIMEOUT (-1) /* 3 seconds */
#define MAX_BUF 64



/****************************************************************
 * gpio_fd_open
 ****************************************************************/

 int gpio_fd_open(unsigned int gpio)
 {
  int fd, len;
  char buf[MAX_BUF];

  len = snprintf(buf, sizeof(buf), SYSFS_GPIO_DIR "/gpio%d/value", gpio);

  fd = open(buf, O_RDONLY | O_NONBLOCK );
  if (fd < 0) {
    perror("gpio/fd_open");
  }
  return fd;
}

/****************************************************************
 * gpio_fd_close
 ****************************************************************/
 int gpio_fd_close(int fd)
 {
  return close(fd);
}

uint64_t compute_time(){
    time_t current_time;
    //char* c_time_string;
 
    /* Obtain current time as seconds elapsed since the Epoch. */
    current_time = time(NULL);

    //printf("currenttime: %lf\n", ((double)current_time-25569.0)*86400.0);
    //return ((uint64_t)((double)current_time-25569.0)*86400.0);
    return (long) current_time;
    /*if (current_time == ((time_t)-1))
    {
        (void) fprintf(stderr, "Failure to compute the current time.");
    }*/
 
    /* Convert to local time format. */
   /* c_time_string = ctime(&current_time);
 
    if (c_time_string == NULL)
    {
        (void) fprintf(stderr, "Failure to convert the current time.");
    }*/
 
    /* Print to stdout. */
   /* (void) printf("Current time is %s", c_time_string);*/
}


uint64_t start_polling(int pin){
  struct pollfd fdset[1];
  int nfds = 1;
  int gpio_fd, timeout, rc;
  char *buf[MAX_BUF];
  int len;
  long t;

  //int count=0;

  gpio_fd = gpio_fd_open(pin);

  timeout = POLL_TIMEOUT;

  //while (count <2) {
    memset((void*)fdset, 0, sizeof(fdset));

    fdset[0].fd = gpio_fd;
    fdset[0].events = POLLPRI;

    read(fdset[0].fd,&buf,64);
    rc = poll(fdset, nfds, timeout);      

    if (rc < 0) {
      printf("\nSomething fails!\n");
    }

    if (rc == 0) {
      printf(".");
    }

    if (fdset[0].revents & POLLPRI && rc > 0) {
      len = read(fdset[0].fd, buf, MAX_BUF);
      t = compute_time();
      //printf("\nGPIO %d interrupted\n", pin);
      //call("GPIO INTERRUPTED");
      
    }
    fflush(stdout);
    //printf("%i\n", count);
    //count++;
  //}
    return t;

  gpio_fd_close(gpio_fd);
}

/*

  ____  ____ ___   _____ _   _ _   _  ____ _____ ___ ___  _   _ ____  
 / ___||  _ |_ _| |  ___| | | | \ | |/ ___|_   _|_ _/ _ \| \ | / ___| 
 \___ \| |_) | |  | |_  | | | |  \| | |     | |  | | | | |  \| \___ \ 
  ___) |  __/| |  |  _| | |_| | |\  | |___  | |  | | |_| | |\  |___) |
 |____/|_|  |___| |_|    \___/|_| \_|\____| |_| |___\___/|_| \_|____/ 
                                                                     

*/

 
/* SPI */
//#define ARRAY_SIZE(a) (sizeof(a) / sizeof((a)[0]))
static const char *device = "/dev/spidev0.0";
static uint8_t mode = 3;
static uint8_t bits = 8;
static uint32_t speed = 1000000;
static uint16_t delay;

static void pabort(const char *s)
{
  perror(s);
  abort();
}

void InternalWriteRead(unsigned char writeBuffer[], int writeOffset, int writeCount, unsigned char readBuffer[], int readOffset, int readCount, int startReadOffset, uint32_t speed)
{
//  printf("### START SPI\n");
  int ret;
  int fd = open(device, O_RDWR);
  if (fd < 0)
    pabort("can't open device");

/*if(writeBuffer != NULL){
  writeBuffer = 0;
  uint8_t tx[writeCount] = writeBuffer;
}

if(readBuffer != NULL){
  readBuffer = 0;
  uint8_t rx[readCount] = readBuffer;
}*/
  /*printf("%s\n", "\nTO SEND:");
  int i;
  for (i = 0; i < size; i++)
  {
      if (i > 0) printf(":");
      printf("%02X", writeBuffer[i]);
  }
*/
  //printf("%s\n", "\nTO SEND:");
 // for (ret = 0; ret < ARRAY_SIZE(writeBuffer); ret++) {
//    if (!(ret % 6))
//      puts("");
  //    printf("%.2X ", writeBuffer[ret]);
//  }

// ret = ioctl(fd, SPI_IOC_WR_MODE, &mode);
//  if (ret == -1)
//    pabort("can't set spi mode");

  
  struct spi_ioc_transfer tr = {
    .tx_buf = (unsigned long)writeBuffer,
    .rx_buf = (unsigned long)readBuffer,
    .len = writeCount,
    .delay_usecs = delay,
    .speed_hz = speed,
    .cs_change = 0,
    .bits_per_word = 8,
  };

  ret = ioctl(fd, SPI_IOC_MESSAGE(1), &tr);
  close(fd);
/*  printf("\nioctl returned: %i\n", ret);
  if (ret < 1)
    printf("%s\n","cna't send spi message");

  printf("%s ", "SPI RETURNS: ");
  for (i = 0; i < ARRAY_SIZE(readBuffer); i++)
  {
      if (i > 0) printf(":");
      printf("%02X", readBuffer[i]);
  }
*/
//  printf("%i\n", ARRAY_SIZE(readBuffer));
 // for (ret = 0; ret < ARRAY_SIZE(readBuffer); ret++) {
//    if (!(ret % 6))
//    printf("%.2X ", readBuffer[ret]);
//  }

 // printf("\n### END SPI\n\n\n\n");
}