def totalMassIteration(engineeringModel, iterationNumber) :
	Command.Clear()
	# Retrieve the iteration
	iteration = Command.GetEngineeringModelIteration(engineeringModel, iterationNumber)
		
	mass = 0 
	# Browse all the element definitions of the iteration
	for elementDefinition in iteration.Element :
		print "\n----------------------------\nelement definition : ", elementDefinition.UserFriendlyShortName
		# Browse all the parameters of the element definition
		for parameter in elementDefinition.Parameter :
			#print "\nparameter : ", parameter.UserFriendlyShortName
			#if parameter.StateDependence is None :
			#	print "StateDependence : null"
			#else :
			#	print "StateDependence : ", parameter.StateDependence
			#print "IsOptionDependent : ", parameter.IsOptionDependent
			# if the parameter is not type of mass we go directly to the next one
			if parameter.UserFriendlyShortName != elementDefinition.UserFriendlyShortName + ".m" :
				continue
			# Add the mass of the parameter to the total mass
			for valueSet in parameter.ValueSet :
				print "published value : ", valueSet.Published[0]				
				if valueSet.Published[0] != "" and "-" not in valueSet.Published[0] :
					val = float(str(valueSet.Published[0]))
					print "%f added to the total mass" %val
					mass += val
	        
	print '\nmass = %f kg' %mass
	return			        

#Command.Clear()
totalMassIteration("MP", 1)