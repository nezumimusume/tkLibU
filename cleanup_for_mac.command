#!/bin/bash
cd `dirname $0`
pushd ./tkLibU_BRP
unlink ./Assets
popd
pushd ./tkLibU_URP
unlink ./Assets
popd

