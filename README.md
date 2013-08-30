[IOSharp - MicroFramework for Linux](http://iosharp.gsole.cat) [![Build Status](https://travis-ci.org/GerardSoleCa/IOSharp-netmf-Linux.png)](https://travis-ci.org/GerardSoleCa/IOSharp-netmf-Linux)
===================

## What is the .NET Micro Framework? ##

>.NET Micro Framework is an open source platform that expands the power and versatility of .NET to the world of small embedded applications. Desktop programmers can harness their existing .NET knowledge base to bring complex embedded concepts to market on time (and under budget). Embedded Developers can tap into the massive productivity gains that have been seen on the Desktop.

>The typical .NET Micro-Framework device has a 32 bit processor with or without a memory management unit (MMU) and could have as little as 64K of random-access memory (RAM). The .NET Micro Framework supports rich user experience and deep connectivity with other devices.

>[Microsoft .NET Micro Framework](http://www.netmf.com/)

## What is the IOSharp - MicroFramework for Linux? ##
The official Micro Framework is designed to be used in resource-constrained devices, as you can read above, so the programs using this framework will run on the bare-metal of the device.

The __IOSharp - MicroFramework for Linux__ is intended to allow applications using the standard Micro Framework run on the top of a Linux operating system. Using this port you will be able to deploy MicroFramework applications to a more powerful device without doing major changes in the code. The only modifications that you will need are changing the references to underlying hardware.

In addition, this port uses the [GPIO, SPI and I2C from Userspace](http://www.haifux.org/lectures/258/gpio_spi_i2c_userspace.pdf) to be distribution and architecture independent.

This project contains two specific parts that are machine-independent, while the third one depends on the deployment device.

Machine-Independent:
-	The Port itself.
-	C library to control SPI and Interruption events.

Specific to machine:

-	HardwareProvider. Linux uses different devices names or mappings, so this must be written to work on the deployment device. Don't worry, it's easy! :)

## Install and use this Port ##

To Do


## Raspberry Pi ##
I'm using a Raspberry Pi to test and deploy MicroFramework programs, so in this repository you will find a HardwareProvider that maps SPI and GPIO ports for the Raspberry Pi (revision 1 and 2).

## State of the Port ##
The Master branch will contain the most *stable* version (this doesn't mean that will not contain bugs or the functionality is totally implemented), while the other branches will contain the development phase of a specific class.
In the following table you can see the current state of the port.

   Master   	| Development 	| To Do 
:--------------:|:-------------:|:------------:
InputPort		|  Serial Port	|   I2C
OutputPort		|       		|  
InterruptPort 	|       		| 
TristatePort	|      			|     
SPI     		|       		|     

[![githalytics.com alpha](https://cruel-carlota.pagodabox.com/e30d5140c6c8e97fc372abbe534873cb "githalytics.com")](http://githalytics.com/GerardSoleCa/IOSharp-netmf-Linux)
