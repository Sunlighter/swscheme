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

;
; This program generates an EXE that displays wall textures from Classic Doom.
; It also works with Doom 2, Heretic, Hexen, and (probably) Strife, because their files are in the same format.
; (You might need to modify it to use "TEXTURES" instead of "TEXTURE1" and "TEXTURE2", though.)
;
; Many variables are defined to #f in order to prevent them from being serialized into the EXE.
; For example, if I used slurp-bytes to load doom.wad before calling generate-exe, the generated exe
; would have an embedded copy of doom.wad.
;

(define doomwadfile "M:\\usb-hd\\33\\FlashFiles\\DOOM\\doom.wad")

(define doomwad #f)

(define dircount #f)

(define dirstart #f)

(define ref-direntry (lambda (ptr)
    (vector
      (byte-ref-uint (byterange doomwad ptr 4))
      (byte-ref-uint (byterange doomwad (+ ptr 4) 4))
      (byte-ref-string (byterange doomwad (+ ptr 8) 8))
      ptr)))

(define findlump (lambda (name)
    (let loop (
        (pos dirstart)
        (count dircount))
      (if (= count 0) #f
        (let* (
            (direntry (ref-direntry pos)))
          (if (string-ci=? (vector-ref direntry 2) name) direntry
            (loop (+ pos 16) (- count 1))))))))

(define findlump2 (lambda (name1 name2)
    (let loop (
        (pos dirstart)
        (count dircount)
        (target (list name1 name2)))
      (if (= count 0) #f
        (let* (
            (direntry (ref-direntry pos)))
          (if (string=? (vector-ref direntry 2) (car target))
            (if (null? (cdr target))
              direntry
              (loop (+ pos 16) (- count 1) (cdr target)))
            (loop (+ pos 16) (- count 1) target)))))))

(define pnames #f)

(define texture1 #f)

(define texture2 #f)

(define texture-list (lambda (lumpinfo)
    (let* (
        (lumpoffset (vector-ref lumpinfo 0))
        (count (byte-ref-uint (byterange doomwad lumpoffset 4))))
      (let loop (
          (the-list '())
          (ptr (+ lumpoffset 4))
          (count count))
        (if (= count 0) (reverse the-list)
          (let* (
              (offset (byte-ref-uint (byterange doomwad ptr 4)))
              (offset2 (+ offset lumpoffset))
              (texname (byte-ref-string (byterange doomwad offset2 8)))
              (tex-x (byte-ref-uint (byterange doomwad (+ offset2 12) 2)))
              (tex-y (byte-ref-uint (byterange doomwad (+ offset2 14) 2)))
              (patchcount (byte-ref-uint (byterange doomwad (+ offset2 20) 2)))
              (patches
                (let loop ((result '()) (remain patchcount) (ptr (+ offset2 22)))
                  (if (= remain 0)
                    (reverse result)
                    (loop
                      (cons (vector
                          (vector-ref pnames (byte-ref-uint (byterange doomwad (+ ptr 4) 2)))
                          (byte-ref-int (byterange doomwad ptr 2))
                          (byte-ref-int (byterange doomwad (+ ptr 2) 2)))
                        result)
                      (- remain 1)
                      (+ ptr 10))))))
            (loop (cons (vector texname tex-x tex-y patches) the-list) (+ ptr 4) (- count 1))))))))

(define draw-post! (lambda (ptr plotyc)
    (let loop ((ptr ptr))
      (let* (
          (start (byte-ref doomwad ptr)))
        (if (= start 255) #t
          (let* (
              (count (byte-ref doomwad (+ ptr 1))))
            (let loop2 ((count count) (y start) (ptr (+ ptr 3)))
              (if (= count 0)
                (loop (+ ptr 1))
                (begin
                  (plotyc y (byte-ref doomwad ptr))
                  (loop2 (- count 1) (+ y 1) (+ ptr 1)))))))))))

(define draw-patch! (lambda (ptr x y plotxyc)
    (let* (
        (width (byte-ref-uint (byterange doomwad ptr 2)))
        (height (byte-ref-uint (byterange doomwad (+ ptr 2) 2))))
      (let loop (
          (ptr1 (+ ptr 8))
          (xx x)
          (count width))
        (if (= count 0) #t
          (let* (
              (post-ptr (+ ptr (byte-ref-uint (byterange doomwad ptr1 4))))
              (plotyc (lambda (yy c) (plotxyc xx (+ yy y) c))))
            (draw-post! post-ptr plotyc)
            (loop (+ ptr1 4) (+ xx 1) (- count 1))))))))

(define playpal0 #f)

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

(define make-bitmap-256 #f)

(define patch->bitmap (lambda (dlist ptr)
    (let* (
        (width (byte-ref-uint (byterange doomwad ptr 2)))
        (height (byte-ref-uint (byterange doomwad (+ ptr 2) 2)))
        (pixels (make-bytes (* width height)))
        (plotxyc (lambda (x y c)
            (if (and (>= x 0) (< x width) (>= y 0) (< y height))
              (byte-set! pixels (+ x (* y width)) c)))))
      (draw-patch! ptr 0 0 plotxyc)
      (make-bitmap-256 (make-byterect pixels 0 width height width) (byterange (big-endian doomwad) playpal0 768)))))

(define texture->bitmap (lambda (tex)
    (let* (
        (width (vector-ref tex 1))
        (height (vector-ref tex 2))
        (pixels (make-bytes (* width height)))
        (plotxyc (lambda (x y c)
            (if (and (>= x 0) (< x width) (>= y 0) (< y height))
              (byte-set! pixels (+ x (* y width)) c))))
        (patches (vector-ref tex 3)))
      (let loop ((patches patches))
        (if (null? patches)
          (make-bitmap-256 (make-byterect pixels 0 width height width) (byterange (big-endian doomwad) playpal0 768))
          (let* (
              (patch (car patches))
              (patch-name (vector-ref patch 0))
              (px (vector-ref patch 1))
              (py (vector-ref patch 2)))
            (draw-patch! (vector-ref (findlump patch-name) 0) px py plotxyc)
            (loop (cdr patches))))))))

(define texture-data #f)

(define all-textures #f)

(define find-texture (lambda (name)
    (let loop ((data texture-data))
      (if (null? data) #f
        (if (string=? (vector-ref (car data) 0) name)
          (car data)
          (loop (cdr data)))))))

(define show-texture! (lambda (name)
    (let* (
        (tdata (find-texture name)))
      (if (not tdata) #f
        (let* (
            (tbitmap (texture->bitmap tdata))
            (ubitmap (make-bitmap 640 480))
            (gr (make-graphics-for-bitmap ubitmap)))
          (graphics-draw-bitmap! gr tbitmap (pointf 32 32))
          (display-bitmap! ubitmap)
          (dispose! gr)
          (dispose! ubitmap)
          (dispose! tbitmap)
          #t)))))

(define main (lambda (a . b)
    (set! doomwad (slurp-bytes doomwadfile))
    (set! dircount (byte-ref-uint (byterange doomwad 4 4)))
    (set! dirstart (byte-ref-uint (byterange doomwad 8 4)))
    (set! texture1 (findlump "TEXTURE1"))
    (set! texture2 (findlump "TEXTURE2"))
    (set! pnames
      (let* (
          (lump (findlump "PNAMES"))
          (offset (vector-ref lump 0))
          (count (byte-ref-uint (byterange doomwad offset 4)))
          (result (make-vector count #f)))
        (let loop ((i 0))
          (if (= i count) result
            (begin
              (vector-set! result i
                (byte-ref-string (byterange doomwad (+ 4 offset (* i 8)) 8)))
              (loop (+ i 1)))))))
    (set! playpal0 (vector-ref (findlump "PLAYPAL") 0))
    (set! make-bitmap-256 (make-bitmap-m 2 2 8 #t))
    (set! texture-data (append (texture-list texture1) (texture-list texture2)))
    (set! all-textures
      (map (lambda (x) (vector-ref x 0)) texture-data))
    (cond
      ((string=? a "list")
        (display-string! (object->string all-textures)))
      ((string=? a "show")
        (show-texture! (car b))))))

(generate-exe "doomshow.exe" main)
