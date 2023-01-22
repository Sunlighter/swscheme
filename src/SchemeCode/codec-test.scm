; This file is part of Sunlit World Scheme
; Copyright (c) 2010 by Edward Kiser (edkiser@gmail.com)
;
; This program is free software; you can redistribute it and/or modify
; it under the terms of the GNU General Public License as published by
; the Free Software Foundation; either version 2 of the License, or
; (at your option) any later version.
;
; This program is distributed in the hope that it will be useful,
; but WITHOUT ANY WARRANTY; without even the implied warranty of
; MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
; GNU General Public License for more details.
;
; You should have received a copy of the GNU General Public License along
; with this program; if not, write to the Free Software Foundation, Inc.,
; 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

; ----
; Note: This file contains snippets to be pasted into the interpreter.
; Loading it is useless.
; ----

;
; List of Ints
;

(define x-read #f)
(define x-can-write? #f)
(define x-write #f)

(make-codec
  '(list-of int)
  (lambda (read can-write? write)
    (set! x-read read)
    (set! x-can-write? can-write?)
    (set! x-write write)))

;
; Generalized List of Ints
;

(define x-read #f)
(define x-can-write? #f)
(define x-write #f)

(make-codec
  '(letrec (
      (item (byte-case (0 (list-of (ref item))) (1 int))))
    (ref item))
  (lambda (read can-write? write)
    (set! x-read read)
    (set! x-can-write? can-write?)
    (set! x-write write)))

(define test-datum '(1 2 (3 4 (5 6)) 7 8 9))

(x-can-write? test-datum)

(define aaa (x-write test-datum))

(dump (byterange aaa))

(x-read aaa)

;
; Generalized List of Symbols
;

(define x-read #f)
(define x-can-write? #f)
(define x-write #f)

(make-codec
  '(letrec (
      (item (byte-case (0 (list-of (ref item))) (1 symbol))))
    (ref item))
  (lambda (read can-write? write)
    (set! x-read read)
    (set! x-can-write? can-write?)
    (set! x-write write)))

(define k1 (gensym))
(define k2 (gensym))
(define test-datum `(pig dog horse ,k1 ,k2 (,k1 pig bear) ,k2))

(x-can-write? test-datum)

(define aaa (x-write test-datum))

(dump (byterange aaa))

(x-read aaa)

;
; Another Generalized List of Symbols
;

(define x-read #f)
(define x-can-write? #f)
(define x-write #f)

(make-codec
  '(letrec (
      (item (byte-case (0 (cons-of (ref item))) (1 symbol) (2 empty-list))))
    (ref item))
  (lambda (read can-write? write)
    (set! x-read read)
    (set! x-can-write? can-write?)
    (set! x-write write)))

(define k1 (gensym))
(define k2 (gensym))
(define test-datum `(pig dog horse ,k1 ,k2 (,k1 pig . bear) ,k2 . ,k1))

(x-can-write? test-datum)

(define aaa (x-write test-datum))

(dump (byterange aaa))

(x-read aaa)

;
; Vector of Ints
;

(define x-read #f)
(define x-can-write? #f)
(define x-write #f)

(make-codec
  '(vector-of int)
  (lambda (read can-write? write)
    (set! x-read read)
    (set! x-can-write? can-write?)
    (set! x-write write)))

(define test-datum '#(100 3 -5 8 68 32))

(x-can-write? test-datum)

(define aaa (x-write test-datum))

(dump (byterange aaa))

(x-read aaa)

;
; The following causes a StackOverflowException when you try to use x-can-write?
;

(define x-read #f)
(define x-can-write? #f)
(define x-write #f)

(make-codec
  '(letrec ((item (ref item))) (ref item))
  (lambda (read can-write? write)
    (set! x-read read)
    (set! x-can-write? can-write?)
    (set! x-write write)))
