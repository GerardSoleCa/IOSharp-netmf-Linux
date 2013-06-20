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

