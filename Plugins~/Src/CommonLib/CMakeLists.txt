CMAKE_MINIMUM_REQUIRED(VERSION 3.13)

project(CommonLib)


file(GLOB sources *.cpp *.h)
set(CommonLib_dir "${CMAKE_CURRENT_SOURCE_DIR}")
if(WIN32)
    file(GLOB win_sources Win/*.h Win/*.cpp)
    list(APPEND sources ${win_sources})    
endif()
set(src_root   "${CMAKE_CURRENT_SOURCE_DIR}/..")
set(plugins_dir "${src_root}/../../Runtime/Plugins/${PLATFORM}")

# setup as a library
add_library(CommonLib STATIC ${sources})
if (LINUX)
    set_property(TARGET CommonLib PROPERTY POSITION_INDEPENDENT_CODE ON)
endif()
    
source_group(TREE ${CMAKE_CURRENT_SOURCE_DIR} FILES ${sources})

#Include, pch
target_precompile_headers(CommonLib PRIVATE "${CommonLib_dir}/stdafx.h")
target_include_directories(CommonLib PRIVATE
    "${CommonLib_dir}/Include"
    "${CommonLib_dir}"
)

