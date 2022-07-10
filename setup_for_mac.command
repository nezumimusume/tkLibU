#!/bin/bash
cd `dirname $0`
pushd ./tkLibU_BRP
ln -s ../tkLibU_Core/Assets ./Assets
popd
pushd ./tkLibU_URP
ln -s ../tkLibU_Core/Assets ./Assets
popd

