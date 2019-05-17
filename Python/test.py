#! /usr/local/bin/python3
import zmq
import sched,time
import random
import numpy as np
def do_PCA(pos):
	print("PCA's")
	print(pos)


s = time.time()
pos = np.array([])
while (time.time()-s)<10:
	response = [1,2,3]
	#print(response)
	#print(pos)
	pos = np.append(pos,response)
	#print(pos)
	#print((time.time()-s)%3)
	if((time.time()-s)%3):
		#do_PCA(pos)
		print('3 second')

	
	



