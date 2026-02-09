#define mUBoo

image_index -= .4;

if(animated_sprite_check_run())
{
        if(image_index > 4 && image_index < 6 && !stepSound)
        {
            	stepSound = true;
		//d3d_instance_create(x+lengthdir_x(5,direction+180),y+lengthdir_y(5,direction+180),z,objSmoke);
        }
	else if(image_index > 0 && image_index < 1)
            	stepSound = false;
}


imageExtraIndex += 5;
flyZ = 2 + lengthdir_y(3,imageExtraIndex);

if(imageExtraIndex > 360)
	imageExtraIndex -= 360;