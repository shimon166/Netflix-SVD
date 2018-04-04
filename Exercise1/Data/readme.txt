SUMMARY
==================================
This set of Yelp data contains 8 categories, which we crawled form Yelp.com website. Most of the users of these categories are from NYC. 
==================================

In each category, there are ratings, users, items, training set and test set file.Besides,we have the file of "user_item_review".

1.All ratings are contained in the file "ratings.txt" and are in the
 following format:

UserID::ItemID::Rating
Ratings are made on a 5-star scale (whole-star ratings only) Each user has at least one ratings.



2.User information is in the file "users.txt" and is in the following
format:


UserID:{Friend1ID,Friend2ID,...,}
Only users who have provided some demographic
 information are included in this data set.
 And to protect users' privacy, we 
reset users' and items' id.



3.Item information is in the file "items.txt" and is in the following
format:


ItemID::Category
The category of items is crawled from Yelp. We give the subcategories and data statistic of each category.

4.The file of "user_item_review" under each category contains each review of a user on an item.
 And to protect users' privacy, we 
reset users' and items' id.

