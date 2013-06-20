#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <unistd.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>
#include <fcntl.h>
#include <poll.h>

/* SPI */
#include <getopt.h>
#include <fcntl.h>
#include <sys/ioctl.h>
#include <linux/types.h>
#include <linux/spi/spidev.h>

 /****************************************************************
 * Constants
 ****************************************************************/
 
#define SYSFS_GPIO_DIR "/sys/class/gpio"
#define POLL_TIMEOUT (-1) /* 3 seconds */
#define MAX_BUF 64

/* SPI */
#define ARRAY_SIZE(a) (sizeof(a) / sizeof((a)[0]))
static const char *device = "/dev/spidev0.1";
static uint8_t mode = 3;
static uint8_t bits = 8;
static uint32_t speed = 1000000;
static uint16_t delay;


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

void init_spi(){
  int ret = 0;
  int fd;

  parse_opts(argc, argv);

  fd = open(device, O_RDWR);
  if (fd < 0)
    pabort("can't open device");

  /*
   * spi mode
   */
  ret = ioctl(fd, SPI_IOC_WR_MODE, &mode);
  if (ret == -1)
    pabort("can't set spi mode");

  ret = ioctl(fd, SPI_IOC_RD_MODE, &mode);
  if (ret == -1)
    pabort("can't get spi mode");

  /*
   * bits per word
   */
  ret = ioctl(fd, SPI_IOC_WR_BITS_PER_WORD, &bits);
  if (ret == -1)
    pabort("can't set bits per word");

  ret = ioctl(fd, SPI_IOC_RD_BITS_PER_WORD, &bits);
  if (ret == -1)
    pabort("can't get bits per word");

  /*
   * max speed hz
   */
  ret = ioctl(fd, SPI_IOC_WR_MAX_SPEED_HZ, &speed);
  if (ret == -1)
    pabort("can't set max speed hz");

  ret = ioctl(fd, SPI_IOC_RD_MAX_SPEED_HZ, &speed);
  if (ret == -1)
    pabort("can't get max speed hz");

  printf("spi mode: %d\n", mode);
  printf("bits per word: %d\n", bits);
  printf("max speed: %d Hz (%d KHz)\n", speed, speed/1000);

  transfer(fd);

  close(fd);

  return ret;
}

void transfer(int fd){
  int ret;
  uint8_t tx[] = {
    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
    0x40, 0x00, 0x00, 0x00, 0x00, 0x95,
    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
    0xDE, 0xAD, 0xBE, 0xEF, 0xBA, 0xAD,
    0xF0, 0x0D,
  };
  uint8_t rx[ARRAY_SIZE(tx)] = {0, };
  struct spi_ioc_transfer tr = {
    .tx_buf = (unsigned long)tx,
    .rx_buf = (unsigned long)rx,
    .len = ARRAY_SIZE(tx),
    .delay_usecs = delay,
    .speed_hz = 0,
    .bits_per_word = 0,
  };

  ret = ioctl(fd, SPI_IOC_MESSAGE(1), &tr);
  if (ret == 1)
    pabort("can't send spi message");

  for (ret = 0; ret < ARRAY_SIZE(tx); ret++) {
    if (!(ret % 6))
      puts("");
    printf("%.2X ", rx[ret]);
  }
  puts("");
}

void InternalWriteRead(uint16_t[] writeBuffer, int writeOffset, int writeCount, uint16_t[] readBuffer, int readOffset, int readCount, int startReadOffset){

}


void InternalWriteRead(uint8_t[] writeBuffer, int writeOffset, int writeCount, uint8_t[] readBuffer, int readOffset, int readCount, int startReadOffset){

}
