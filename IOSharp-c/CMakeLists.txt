cmake_minimum_required(VERSION 2.6)
project(NETMF-LINUX)

OPTION(IOSharp-c "Build the IOSharp-C library shared." ON)

if(IOSharp-c)
	SET(IOSharp-c_LIBRARY_TYPE SHARED)
endif(IOSharp-c)


add_library(IOSharp-c ${IOSharp-c_LIBRARY_TYPE} IOSharp.c)
install(TARGETS IOSharp-c DESTINATION /usr/lib)
