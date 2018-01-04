# Under development (not ready for usage yet)
# What?
Simple WPF application to get metadata (title, author, publisher, edition, description, etc.) for books from their ISBN.
	
This application was originally created to JUST get the metadata and make it easily available to 
	import into a library system.
	
##Main use case scenario
1. Get the ISBN through the **book's barcode** using a VNC-app in your mobile device + desktop
 * I used [SCANPET] (https://play.google.com/store/apps/details?id=com.maiko.scanpet&hl=en), a free Android app 
2. **Add the ISBN** into the application
 * Two possibilities here: (1) add into the DataGrid or (2) through a CSV file.
3. Start the **crawling**
4. Use the output data
 * CSV
 * Copy-and-paste from the DataGrid

# Why?
I got frustrated at my vacation when trying to find a book in my home library because it took too much time… So I saw an opportunity to make a simple crawler to get the book's metadata quicker than by hand.

Also at work (Habits Incubator-School) we have a small library that is not digitalized and getting the metadata from the books quicker would save us some time.
	

# How?
WPF application that crawls Amazon, Google and other book websites.
	
## Why WPF?
1. At my vacation I was with limited (small data plan) connection and the best option was to use the tools I already have (Visual Studio).
2. I really like it :)
		
## Why not a ready-to-use API?
I searched for a ready-to-use API but Amazon's is for "affiliates" only (people that wanna sell their products) and the other ones are not free.

Google's one is free but it is not as complete as Amazon's.