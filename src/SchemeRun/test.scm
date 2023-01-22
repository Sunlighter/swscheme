;
; Copyright (c) 2010 by Edward Kiser
; Licensed under the GNU General Public License version 2 (or any later version)
; All Rights Reserved Except as Specified Therein
;

(display-string! "Hello!\r\n")

(display-string! "Number of arguments: ")

(display-string! (object->string (vector-length args)))

(display-string! "\r\n")

(display-string! (object->string args))

(display-string! "\r\n")

(define t1 (pascalesque
    '(lambda ((int x) (int y))
      (let (((tuple int int) r (new (tuple int int) (int int) x y)))
        (tuple-first r)))))
