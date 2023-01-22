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

(define make-bitmap-m (lambda (xzoom yzoom bpp hbf)
    (letrec (
        (scx (lambda () `(* (int ,(* xzoom (/ 8 bpp))) (byterect-get-width pixels))))
        (scy (lambda () `(* (int ,yzoom) (byterect-get-height pixels))))
        (main (lambda ()
            `(let (
                (int y (int 0))
                (int yEnd (byterect-get-height pixels))
                (int srcPtr (byterect-get-offset pixels))
                (int srcPtrPlus (byterect-get-stride pixels))
                (intptr destPtr (bitmapdata-get-scan0 lockedBits))
                (intptr destPtrPlus (to-intptr (bitmapdata-get-stride lockedBits)))
                ((array-of byte) arr (sba-get-bytes (byterect-get-sba pixels))))
              ,(main0))))
        (main0 (lambda ()
            `(while
              (nop)
              (< y yEnd)
              (begin
                ,(main1)
                (set! srcPtr (+ srcPtr srcPtrPlus))
                (set! y (+ y (int 1)))))))
        (main1 (lambda ()
            (if (= yzoom 1)
              `(begin
                ,(main2)
                (set! destPtr (+ destPtr destPtrPlus)))
              `(let (
                  (int iy (int 0)))
                (while
                  (nop)
                  (< iy (int ,yzoom))
                  (begin
                    ,(main2)
                    (set! iy (+ iy (int 1)))
                    (set! destPtr (+ destPtr destPtrPlus))))))))
        (main2 (lambda ()
            `(let* (
                (int srcPtr0 srcPtr)
                (int srcPtr0end (+ srcPtr0 (byterect-get-width pixels)))
                (intptr destPtr0 destPtr))
              (while
                (nop)
                (< srcPtr0 srcPtr0end)
                (begin
                  ,(main3)
                  (set! srcPtr0 (+ srcPtr0 (int 1))))))))
        (main3 (lambda ()
            `(let* (
                (byte pix (array-ref arr srcPtr0)))
              ,(main4))))
        (main4 (lambda ()
            (cond
              ((= bpp 8)
                `(let* (
                    (int outpix (array-ref palette (as-int pix))))
                  ,@(main5)))
              ((and (= bpp 4) hbf)
                `(let* (
                    (int outpix (array-ref palette (as-int (logand (shr pix (int 4)) (byte 15))))))
                  ,@(main5)
                  (set! outpix (array-ref palette (as-int (logand pix (byte 15)))))
                  ,@(main5)))
              ((and (= bpp 4) (not hbf))
                `(let* (
                    (int outpix (array-ref palette (as-int (logand pix (byte 15))))))
                  ,@(main5)
                  (set! outpix (array-ref palette (as-int (logand (shr pix (int 4)) (byte 15)))))
                  ,@(main5)))
              ((and (= bpp 2) hbf)
                `(let* (
                    (int shf (int 6))
                    (int outpix (int 0)))
                  (while
                    (nop)
                    (>= shf (int 0))
                    (begin
                      (set! outpix (array-ref palette (as-int (logand (shr pix shf) (byte 3)))))
                      ,@(main5)
                      (set! shf (- shf (int 2)))))))
              ((and (= bpp 2) (not hbf))
                `(let* (
                    (int shf (int 0))
                    (int outpix (int 0)))
                  (while
                    (nop)
                    (< shf (int 8))
                    (begin
                      (set! outpix (array-ref palette (as-int (logand (shr pix shf) (byte 3)))))
                      ,@(main5)
                      (set! shf (+ shf (int 2)))))))
              ((and (= bpp 1) hbf)
                `(let* (
                    (int shf (int 7))
                    (int outpix (int 0)))
                  (while
                    (nop)
                    (>= shf (int 0))
                    (begin
                      (set! outpix (array-ref palette (as-int (logand (shr pix shf) (byte 1)))))
                      ,@(main5)
                      (set! shf (- shf (int 1)))))))
              ((and (= bpp 1) (not hbf))
                `(let* (
                    (int shf (int 0))
                    (int outpix (int 0)))
                  (while
                    (nop)
                    (< shf (int 8))
                    (begin
                      (set! outpix (array-ref palette (as-int (logand (shr pix shf) (byte 1)))))
                      ,@(main5)
                      (set! shf (+ shf (int 1)))))))
              (else (throw "Bad bpp")))))
        (main5 (lambda ()
            (cond
              ((= xzoom 1)
                `((poke! destPtr0 outpix) (set! destPtr0 (+ destPtr0 (to-intptr (int 4))))))
              ((= xzoom 2)
                `((poke! destPtr0 outpix) (set! destPtr0 (+ destPtr0 (to-intptr (int 4))))
                  (poke! destPtr0 outpix) (set! destPtr0 (+ destPtr0 (to-intptr (int 4))))))
              (else
                `((let* (
                      (int ix (int 0)))
                    (while
                      (nop)
                      (< ix (int ,xzoom))
                      (begin
                        (poke! destPtr0 outpix)
                        (set! destPtr0 (+ destPtr0 (to-intptr (int 4))))
                        (set! ix (+ ix (int 1))))))))))))
      (make-bitmap-maker
        (scx)
        (scy)
        (main)))))

(define xsize 1024)

(define ysize 768)

(define rbx (make-bytes (* xsize ysize)))

(define for (lambda (s e proc)
    (let loop ((i s)) (if (>= i e) #t (begin (proc i) (loop (+ i 1)))))))

(for 0 4096 (lambda (x) (byte-set! rbx x x)))

(define pset (lambda (m)
    (case (logand m 3)
      ((0) (lambda (rect x y c)
          (let* (
              (xbyte (shr x 3))
              (xbit (shl 1 (logand x 7)))
              (addr (+ (get-byterect-offset rect) (* (get-byterect-stride rect) y) xbyte))
              (array (get-byterect-array rect)))
            (if (= (logand c 1) 0)
              (byte-set! array addr (logand (lognot xbit) (byte-ref array addr)))
              (byte-set! array addr (logior xbit (byte-ref array addr)))))))
      ((1) (lambda (rect x y c)
          (let* (
              (xbyte (shr x 2))
              (xbit (shl 3 (* 2 (logand x 3))))
              (cbit (shl c (* 2 (logand x 3))))
              (addr (+ (get-byterect-offset rect) (* (get-byterect-stride rect) y) xbyte))
              (array (get-byterect-array rect)))
            (byte-set! array addr (logior cbit (logand (lognot xbit) (byte-ref array addr)))))))
      ((2) (lambda (rect x y c)
          (let* (
              (xbyte (shr x 1))
              (xbit (shl 15 (* 4 (logand x 1))))
              (cbit (shl c (* 4 (logand x 1))))
              (addr (+ (get-byterect-offset rect) (* (get-byterect-stride rect) y) xbyte))
              (array (get-byterect-array rect)))
            (byte-set! array addr (logior cbit (logand (lognot xbit) (byte-ref array addr)))))))
      (else (lambda (rect x y c)
          (let* (
              (addr (+ (get-byterect-offset rect) (* (get-byterect-stride rect) y) x))
              (array (get-byterect-array rect)))
            (byte-set! array addr c)))))))

(fill-byterect! (make-byterect rbx 0 1024 768 1024) 128)

(let* (
    (pset3 (pset 3))
    (rect (make-byterect rbx 0 1024 768 1024)))
  (for 0 65536 (lambda (x)
      (pset3 rect (random-int 1024) (random-int 768) (random-int 256)))))

(define v-bitmap-maker (make-vector 64 #f))

(define v-bitmap (make-vector 64 #f))

(define pal (vector
    (bytes 0 0 0 255 255 255)
    (bytes 0 0 0 85 85 85 170 170 170 255 255 255)
    (let* (
        (pal3 (make-bytes 48)))
      (let loop ((i 0))
        (if (= i 16) pal3
          (let* (
              (j (* 17 i))
              (idx (* 3 i)))
            (byte-set! pal3 idx j)
            (byte-set! pal3 (+ idx 1) j)
            (byte-set! pal3 (+ idx 2) j)
            (loop (+ i 1))))))
    (let* (
        (pal4 (make-bytes 768)))
      (let loop ((i 0))
        (if (= i 256) pal4
          (let* (
              (idx (* 3 i)))
            (byte-set! pal4 idx i)
            (byte-set! pal4 (+ idx 1) i)
            (byte-set! pal4 (+ idx 2) i)
            (loop (+ i 1))))))
    ))

(for 0 64 (lambda (mode)
    (let (
        (xscale (let* ((mx (logand mode 48)))
            (cond
              ((= 48 mx) 1)
              ((= 32 mx) 2)
              ((= 16 mx) 4)
              (else 8))))
        (yscale (let* ((mx (logand mode 12)))
            (cond
              ((= 12 mx) 1)
              ((= 8 mx) 2)
              ((= 4 mx) 4)
              (else 8))))
        (bpp (let* ((mx (logand mode 3)))
            (cond
              ((= 3 mx) 8)
              ((= 2 mx) 4)
              ((= 1 mx) 2)
              (else 1))))
        (pal1 (vector-ref pal (logand mode 3))))
      (let (
          (make-it (make-bitmap-m xscale yscale bpp #f))
          (stride (/ (* xsize bpp) (* 8 xscale))))
        (vector-set! v-bitmap-maker mode make-it)
        (vector-set! v-bitmap mode
          (make-it (make-byterect rbx 0 stride (/ ysize yscale) stride) (byterange pal1)))))))

(define w (make-window-obj-from-bitmap (vector-ref v-bitmap 63)))

(define show! (lambda (x) (post! w #msg(set-image . image (vector-ref v-bitmap x)))))

(define x1024 48)
(define x512 32)
(define x256 16)
(define x128 0)
(define y768 12)
(define y384 8)
(define y192 4)
(define y96 0)
(define c256 3)
(define c16 2)
(define c4 1)
(define c2 0)
