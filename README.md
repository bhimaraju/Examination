This project was made as part of an academic project where we were supposed to create an examination application whose data could be loaded through XML.

A sample XML containing three examinations is present in the source. The exam generates options to select an exam, loads the exam to the user and reads inputs and grades accordingly.

We dealt with essay questions by bypassing any NLP. This was implemented by assuming that each "essay" question had a second part which specifically tests for a concept which is critical to the theory. The input is taken as a fill in the blank. However, we use some online NLP APIs to get a concept from the "theory" part of the answer to ensure that the candidate did put some efforts there as well.

Using Winforms was not a choice for us. It was part of the project requirement.

Deepak Bhimaraju, Rashid Yakubu, Hillary Thompson - MS Information Systems, University of Cincinnati